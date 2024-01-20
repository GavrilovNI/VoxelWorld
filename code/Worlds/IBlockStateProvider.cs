using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IBlockStateProvider
{
    // Thread safe
    BlockState GetBlockState(Vector3Int position);
}
