

using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IWorldProvider : IBlockStateProvider
{
    Vector3Int GetBlockPosition(Vector3 position);
    Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal);
    Vector3 GetBlockWorldPosition(Vector3Int position);

    Vector3Int GetChunkPosition(Vector3Int blockPosition);
    Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition);
}
