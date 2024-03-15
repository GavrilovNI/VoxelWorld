using VoxelWorld.Blocks.Entities;
using VoxelWorld.Mth;

namespace VoxelWorld.Worlds;

public interface IBlockEntityProvider
{
    // Thread safe
    BlockEntity? GetBlockEntity(Vector3Int blockPosition);
}
