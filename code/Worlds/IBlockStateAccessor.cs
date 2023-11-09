using Sandcube.Mth;
using Sandcube.Worlds.Blocks;

namespace Sandcube.Worlds;

public interface IBlockStateAccessor : IBlockStateProvider
{
    void SetBlockState(Vector3Int position, BlockState blockState);
}
