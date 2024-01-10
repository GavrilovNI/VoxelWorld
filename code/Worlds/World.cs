using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Worlds.Generation;
using System;
using System.Collections.Generic;

namespace Sandcube.Worlds;

public class World : Component, IWorldAccessor
{
    [Property] public Vector3Int ChunkSize { get; init; } = 16;
    [Property] public Material OpaqueVoxelsMaterial { get; set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; set; } = null!;
    [Property] public WorldGenerator? Generator { get; set; }

    [Property] public PrefabFile ChunkPrefab { get; set; } = null!;

    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();


    protected virtual Chunk AddChunk(Vector3Int position)
    {
        if(_chunks.ContainsKey(position))
            throw new InvalidOperationException($"{nameof(Chunk)} aready exists in position {position}");

        var chunkGameObject = new GameObject(false, $"Chunk {position}");
        chunkGameObject.SetPrefabSource(ChunkPrefab.ResourcePath);
        chunkGameObject.UpdateFromPrefab();
        chunkGameObject.BreakFromPrefab();

        chunkGameObject.Parent = this.GameObject;
        chunkGameObject.Transform.LocalRotation = Rotation.Identity;
        chunkGameObject.Transform.LocalPosition = position * ChunkSize * MathV.InchesInMeter;

        var chunk = chunkGameObject.Components.Get<Chunk>(true);
        chunk.Initialize(position, ChunkSize, this);

        chunk.OpaqueVoxelsMaterial = OpaqueVoxelsMaterial;
        chunk.TranslucentVoxelsMaterial = TranslucentVoxelsMaterial;

        var proxies = chunkGameObject.Components.GetAll<WorldProxy>(FindMode.DisabledInSelfAndDescendants);
        foreach(var proxy in proxies)
            proxy.World = this;

        _chunks.Add(position, chunk);

        chunkGameObject.Enabled = true;
        return chunk;
    }

    protected virtual bool TryGenerateChunk(Chunk chunk)
    {
        if(Generator is null)
            return false;

        Generator.GenerateChunk(chunk);
        UpdateNeighboringChunks(chunk.Position);
        return true;
    }

    protected virtual Chunk CreateChunk(Vector3Int position)
    {
        var chunk = AddChunk(position);
        TryGenerateChunk(chunk);
        return chunk;
    }

    public virtual Chunk? GetChunk(Vector3Int position, bool forceLoad = false)
    {
        if(!_chunks.TryGetValue(position, out var chunk) && forceLoad)
            chunk = CreateChunk(position);
        return chunk;
    }

    public virtual Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal)
    {
        hitNormal = Transform.World.NormalToLocal(hitNormal.Normal);
        var result = Transform.World.PointToLocal(position).Divide(MathV.InchesInMeter);

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
    public virtual Vector3Int GetBlockPosition(Vector3 position) => Transform.World.PointToLocal(position).Divide(MathV.InchesInMeter).Floor();

    public virtual Vector3Int GetChunkPosition(Vector3 position) => GetChunkPosition(GetBlockPosition(position));
    public virtual Vector3 GetBlockGlobalPosition(Vector3Int position) => position * MathV.InchesInMeter;

    public virtual Vector3Int GetChunkPosition(Vector3Int blockPosition) => blockPosition.WithAxes((a, v) => (int)MathF.Floor(((float)v) / ChunkSize.GetAxis(a)));
    public virtual Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => (blockPosition % ChunkSize + ChunkSize) % ChunkSize;
    public virtual Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => chunkPosition * ChunkSize + blockLocalPosition;


    public virtual void SetBlockState(Vector3Int position, BlockState blockState)
    {
        var oldBlockState = GetBlockState(position);
        if(oldBlockState == blockState)
            return;

        var chunkPosition = GetChunkPosition(position);
        var chunk = GetChunk(chunkPosition, true)!;
        var localPosition = GetBlockPositionInChunk(position);
        chunk.SetBlockState(localPosition, blockState);

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
            var chunk = GetChunk(chunkPosition, false);
            chunk?.OnNeighbouringChunkEdgeUpdated(direction.GetOpposite(), updatedBlockPosition, oldBlockState, newBlockState);
        }
        return true;
    }

    protected virtual void UpdateNeighboringChunks(Vector3Int chunkPosition)
    {
        foreach(Direction direction in Direction.All)
        {
            var chunk = GetChunk(chunkPosition + direction, false);
            if(chunk is not null)
                chunk.ModelsRebuildRequired = true;
        }    
    }

    public virtual BlockState GetBlockState(Vector3Int position)
    {
        var chunkPosition = GetChunkPosition(position);
        var chunk = GetChunk(chunkPosition, false)!;
        if(chunk is null)
            return BlockState.Air;
        position = GetBlockPositionInChunk(position);
        return chunk.GetBlockState(position);
    }

    public virtual void Clear()
    {
        foreach(var chunk in _chunks)
            chunk.Value.GameObject.Destroy();
        _chunks.Clear();
    }
}
