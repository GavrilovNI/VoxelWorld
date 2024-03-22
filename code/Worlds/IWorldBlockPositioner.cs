using VoxelWorld.Mth;

namespace VoxelWorld.Worlds;

public interface IWorldBlockPositioner
{
    Vector3IntB GetBlockPosition(Vector3 position);
    Vector3IntB GetBlockPosition(Vector3 position, Vector3 hitNormal);
    Vector3 GetBlockGlobalPosition(Vector3IntB blockPosition);

    Vector3IntB GetChunkPosition(Vector3 position);
    Vector3IntB GetChunkPosition(Vector3IntB blockPosition);
    Vector3IntB GetBlockPositionInChunk(Vector3IntB blockPosition);
    Vector3IntB GetBlockWorldPosition(Vector3IntB chunkPosition, Vector3IntB blockLocalPosition);
}
