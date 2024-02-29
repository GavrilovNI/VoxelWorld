using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IWorldProvider : IBlockStateProvider, IBlockEntityProvider, IWorldBlockPositioner
{
    bool HasChunk(Vector3Int chunkPosition);
    bool IsChunkInLimits(Vector3Int chunkPosition);
}
