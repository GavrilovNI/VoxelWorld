using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IWorldBlockPositioner
{
    Vector3Int GetBlockPosition(Vector3 position);
    Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal);
    Vector3 GetBlockGlobalPosition(Vector3Int blockPosition);

    Vector3Int GetChunkPosition(Vector3 position);
    Vector3Int GetChunkPosition(Vector3Int blockPosition);
    Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition);
    Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition);
}
