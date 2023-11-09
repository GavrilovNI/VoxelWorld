using Sandbox;
using Sandcube.Mth;
using Sandcube.Scene.Extensions;
using Sandcube.Worlds.Blocks;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds;

public class World : BaseComponent, IBlockStateAccessor
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
