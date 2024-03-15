using Sandcube.Mth;
using Sandcube.Registries;
using System;

namespace Sandcube.Worlds;

public interface IWorldProvider : IBlockStateProvider, IBlockEntityProvider, IWorldBlockPositioner
{
    event Action<Vector3Int>? ChunkLoaded;
    event Action<Vector3Int>? ChunkUnloaded;

    ModedId Id { get; }
    BBoxInt Limits { get; }

    bool HasChunk(Vector3Int chunkPosition);
    bool IsChunkInLimits(Vector3Int chunkPosition);
}
