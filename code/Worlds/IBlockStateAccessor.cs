using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds;

public interface IBlockStateAccessor : IBlockStateProvider
{
    Task<BlockStateChangingResult> SetBlockState(Vector3Int blockPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.Default);
}
