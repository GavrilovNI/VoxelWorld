using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor
{
    Chunk? GetChunk(Vector3Int position, bool forceLoad = false);
}
