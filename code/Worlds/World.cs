using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Threading;
using Sandcube.Worlds.Loading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class World : ThreadHelpComponent, IWorldAccessor
{
    [Property] public Vector3Int ChunkSize { get; init; } = 16;
    [Property] protected ChunkLoader ChunkLoader { get; set; } = null!;


    protected readonly Dictionary<Vector3Int, OneOf<Chunk, Task<Chunk>>> Chunks = new();
    protected CancellationTokenSource ChunkLoadCancellationTokenSource = new();

    protected override void OnDestroyInner()
    {
        base.OnDestroyInner();
        Clear();
    }

    // Thread safe
    public virtual Task<Chunk> GetChunkOrLoad(Vector3Int position)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(position, out var chunkUnion))
            {
                if(chunkUnion.Is<Chunk>(out var chunk))
                    return Task.FromResult(chunk);
                return chunkUnion.As<Task<Chunk>>()!;
            }

            ChunkCreationData creationData = new()
            {
                Position = position,
                Size = ChunkSize,
                EnableOnCreate = false,
                World = this
            };

            var loadingTask = ChunkLoader.LoadChunk(creationData, ChunkLoadCancellationTokenSource.Token);
            Chunks[position] = loadingTask;
            _ = loadingTask.ContinueWith(t =>
            {
                if(!IsValid)
                    return;
                lock(Chunks)
                {
                    if(t.IsCompletedSuccessfully)
                    {
                        var chunk = t.Result;
                        Chunks[position] = chunk;

                        _ = RunInGameThread(ct =>
                        {
                            chunk.GameObject.Enabled = true;
                            OnChunkLoaded(chunk);
                        });
                    }
                    else
                    {
                        Chunks.Remove(position);
                    }
                }
            });

            return loadingTask;
        }
    }

    // Call only in game thread
    protected virtual void OnChunkLoaded(Chunk chunk)
    {
        UpdateNeighboringChunks(chunk.Position);
    }

    // Thread safe
    public virtual Chunk? GetChunk(Vector3Int position)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(position, out var chunkUnion) && chunkUnion.Is<Chunk>(out var chunk))
                return chunk;
        }
        return null;
    }

    public virtual Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal)
    {
        hitNormal = Transform.World.NormalToLocal(hitNormal.Normal);
        var result = Transform.World.PointToLocal(position).Divide(MathV.UnitsInMeter);

        foreach(var axis in Axis.All)
        {
            var mod = result.GetAxis(axis) % 1;
            var hitAxis = hitNormal.GetAxis(axis);

            if(hitAxis > 0)
            {
                if((mod.AlmostEqual(0) && mod >= 0) || mod.AlmostEqual(-1))
                    result = result.WithAxis(axis, result.GetAxis(axis) - 1);
            }
            else if(mod.AlmostEqual(1))
            {
                result = result.WithAxis(axis, result.GetAxis(axis) + 1);
            }
        }

        return result.Floor();
    }
    public virtual Vector3Int GetBlockPosition(Vector3 position) => Transform.World.PointToLocal(position).Divide(MathV.UnitsInMeter).Floor();

    public virtual Vector3Int GetChunkPosition(Vector3 position) => GetChunkPosition(GetBlockPosition(position));
    public virtual Vector3 GetBlockGlobalPosition(Vector3Int position) => position * MathV.UnitsInMeter;

    public virtual Vector3Int GetChunkPosition(Vector3Int blockPosition) => blockPosition.WithAxes((a, v) => (int)MathF.Floor(((float)v) / ChunkSize.GetAxis(a)));
    public virtual Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => (blockPosition % ChunkSize + ChunkSize) % ChunkSize;
    public virtual Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => chunkPosition * ChunkSize + blockLocalPosition;


    // Thread safe
    public virtual async Task SetBlockState(Vector3Int position, BlockState blockState)
    {
        var chunkPosition = GetChunkPosition(position);
        var chunk = await GetChunkOrLoad(chunkPosition);

        if(!chunk.IsValid())
            throw new InvalidOperationException($"Couldn't load {nameof(Chunk)} at position {chunkPosition}");

        var localPosition = GetBlockPositionInChunk(position);

        var oldBlockState = chunk.GetBlockState(localPosition);
        if(oldBlockState == blockState)
            return;

        await chunk.SetBlockState(localPosition, blockState);

        NotifyNeighboringChunksAboutEdgeUpdate(position, oldBlockState, blockState);
    }

    protected virtual List<Direction> GetNeighboringChunkDirections(Vector3Int localBlockPosition)
    {
        List<Direction> result = new();

        if(localBlockPosition.x == 0)
            result.Add(Direction.Backward);
        if(localBlockPosition.x == ChunkSize.x - 1)
            result.Add(Direction.Forward);
        if(localBlockPosition.y == 0)
            result.Add(Direction.Right);
        if(localBlockPosition.y == ChunkSize.y - 1)
            result.Add(Direction.Left);
        if(localBlockPosition.z == 0)
            result.Add(Direction.Down);
        if(localBlockPosition.z == ChunkSize.z - 1)
            result.Add(Direction.Up);

        return result;
    }

    protected virtual bool NotifyNeighboringChunksAboutEdgeUpdate(Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        var localBlockPosition = GetBlockPositionInChunk(updatedBlockPosition);
        List<Direction> neighboringChunkDirections = GetNeighboringChunkDirections(localBlockPosition);
        if(neighboringChunkDirections.Count == 0)
            return false;

        foreach(Direction direction in neighboringChunkDirections)
        {
            var neighborBlockPosition = updatedBlockPosition + direction;
            var chunkPosition = GetChunkPosition(neighborBlockPosition);
            var chunk = GetChunk(chunkPosition);
            chunk?.OnNeighbouringChunkEdgeUpdated(direction.GetOpposite(), updatedBlockPosition, oldBlockState, newBlockState);
        }
        return true;
    }

    protected virtual void UpdateNeighboringChunks(Vector3Int chunkPosition)
    {
        foreach(Direction direction in Direction.All)
        {
            var neighboringChunkPosition = chunkPosition + direction;
            var chunk = GetChunk(neighboringChunkPosition);
            chunk?.RequireModelUpdate();
        }    
    }

    // Thread safe
    public virtual BlockState GetBlockState(Vector3Int position)
    {
        var chunkPosition = GetChunkPosition(position);
        var chunk = GetChunk(chunkPosition);
        if(chunk is null)
            return BlockState.Air;
        position = GetBlockPositionInChunk(position);
        return chunk.GetBlockState(position);
    }

    // Thread safe
    public virtual void Clear()
    {
        lock(Chunks)
        {
            ChunkLoadCancellationTokenSource.Cancel();
            ChunkLoadCancellationTokenSource.Dispose();
            ChunkLoadCancellationTokenSource = new();

            foreach(var chunkUnion in Chunks.Values)
                chunkUnion.As<Chunk>()?.GameObject.Destroy();

            Chunks.Clear();
        }
    }
}
