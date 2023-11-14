using Sandcube.Mth;
using Sandcube.Worlds.Blocks.States;

namespace Sandcube.Worlds;

public interface IBlockStateProvider
{
    BlockState GetBlockState(Vector3Int position);
}
