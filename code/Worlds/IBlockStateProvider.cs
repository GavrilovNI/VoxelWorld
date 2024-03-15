using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;

namespace VoxelWorld.Worlds;

public interface IBlockStateProvider
{
    // Thread safe
    BlockState GetBlockState(Vector3Int blockPosition);
}
