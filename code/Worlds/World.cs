using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Scenes.Extensions;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds;

public class World : BaseComponent, IWorldAccessor
{
    [Property] public Vector3 VoxelSize { get; init; } = Vector3.One * 39.37f;
    [Property] public Vector3Int ChunkSize { get; init; } = 16;
    [Property] public Material VoxelsMaterial { get; set; } = null!;
    [Property] public WorldGenerator Generator { get; set; } = null!;

    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();

    public override void OnStart()
    {
        if(Generator is null)
            Generator = GetComponent<WorldGenerator>();
    }


    protected virtual Chunk CreateChunk(Vector3Int position)
    {
        var chunkGameObject = Scene.CreateObject();
        chunkGameObject.Name = $"Chunk {position}";
        chunkGameObject.Parent = this.GameObject;
        chunkGameObject.Transform.Position = position * VoxelSize * ChunkSize;
        chunkGameObject.Tags.Add("world");

        var chunk = new Chunk(position, ChunkSize, VoxelSize)
        {
            VoxelsMaterial = VoxelsMaterial
        };
        chunkGameObject.AddComponent(chunk);
        return chunk;
    }

    public virtual Chunk GenerateChunk(Vector3Int position)
    {
        var chunk = CreateChunk(position);
        Generator?.GenerateChunk(chunk);
        _chunks[position] = chunk;
        return chunk;
    }

    public virtual Chunk? GetChunk(Vector3Int position, bool forceLoad = false)
    {
        if(!_chunks.TryGetValue(position, out var chunk) && forceLoad)
            chunk = GenerateChunk(position);
        return chunk;
    }

    public virtual Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal)
    {
        hitNormal = hitNormal.Normal;
        var result = position.Divide(VoxelSize);

        foreach(var axis in Axis.All)
        {
            var mod = result.GetAxis(axis) % 1;
            var hitAxis = hitNormal.GetAxis(axis);

            if(hitAxis > 0)
            {
                if(mod.AlmostEqual(0) || mod.AlmostEqual(-1))
                    result = result.WithAxis(axis, result.GetAxis(axis) - 1);
            }
            else if(mod.AlmostEqual(1))
            {
                result = result.WithAxis(axis, result.GetAxis(axis) + 1);
            }
        }

        return result.Floor();
    }
    public virtual Vector3Int GetBlockPosition(Vector3 position) => position.Divide(VoxelSize).Floor();
    public virtual Vector3Int GetChunkPosition(Vector3 position) => GetChunkPosition(GetBlockPosition(position));
    public virtual Vector3 GetBlockWorldPosition(Vector3Int position) => position * VoxelSize;

    public virtual Vector3Int GetChunkPosition(Vector3Int blockPosition) => blockPosition.WithAxes((a, v) => (int)MathF.Floor(((float)v) / ChunkSize.GetAxis(a)));
    public virtual Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => (blockPosition % ChunkSize + ChunkSize) % ChunkSize;


    public virtual void SetBlockState(Vector3Int position, BlockState blockState)
    {
        var chunkPosition = GetChunkPosition(position);
        var chunk = GetChunk(chunkPosition, true)!;
        position = GetBlockPositionInChunk(position);
        chunk.SetBlockState(position, blockState);
    }

    public virtual BlockState GetBlockState(Vector3Int position)
    {
        var chunkPosition = GetChunkPosition(position);
        var chunk = GetChunk(chunkPosition, true)!;
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
