using VoxelWorld.Mth;
using VoxelWorld.Registries;
using System;

namespace VoxelWorld.Worlds;

public interface IWorldProvider : IBlockStateProvider, IBlockEntityProvider, IWorldBlockPositioner
{
    event Action<Vector3IntB>? ChunkLoaded;
    event Action<Vector3IntB>? ChunkUnloaded;

    ModedId Id { get; }
    BBoxInt Limits { get; }

    bool HasChunk(Vector3IntB chunkPosition);
    bool IsChunkInLimits(Vector3IntB chunkPosition);
}
