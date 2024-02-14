using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public interface IBlockStateAccessor : IBlockStateProvider
{
    Task<BlockStateChangingResult> SetBlockState(Vector3Int blockPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.Default);
}
