using VoxelWorld.Mth;
using VoxelWorld.Registries;
using System;
using VoxelWorld.Data;

namespace VoxelWorld.Worlds;

public interface IWorldProvider : IBlockStateProvider, IBlockEntityProvider, IWorldBlockPositioner
{
    event Action<Vector3IntB>? ChunkLoaded;
    event Action<Vector3IntB>? ChunkUnloaded;

    ModedId Id { get; }
    BBoxInt Limits { get; }
    WorldOptions Options { get; }

    bool HasChunk(Vector3IntB chunkPosition);
    bool IsChunkInLimits(Vector3IntB chunkPosition);
}
