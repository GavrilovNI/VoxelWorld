using VoxelWorld.Mth;
using VoxelWorld.Registries;
using System;

namespace VoxelWorld.Worlds;

public interface IWorldProvider : IBlockStateProvider, IBlockEntityProvider, IWorldBlockPositioner
{
    event Action<Vector3Int>? ChunkLoaded;
    event Action<Vector3Int>? ChunkUnloaded;

    ModedId Id { get; }
    BBoxInt Limits { get; }

    bool HasChunk(Vector3Int chunkPosition);
    bool IsChunkInLimits(Vector3Int chunkPosition);
}
