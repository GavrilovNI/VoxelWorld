using Sandcube.Mth;
using Sandcube.Worlds.Blocks.States;

namespace Sandcube.Worlds;

public interface IBlockStateAccessor : IBlockStateProvider
{
    void SetBlockState(Vector3Int position, BlockState blockState);
}
