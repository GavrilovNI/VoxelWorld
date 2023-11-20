using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IBlockStateAccessor : IBlockStateProvider
{
    void SetBlockState(Vector3Int position, BlockState blockState);
}
