using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IBlockStateProvider
{
    BlockState GetBlockState(Vector3Int position);
}
