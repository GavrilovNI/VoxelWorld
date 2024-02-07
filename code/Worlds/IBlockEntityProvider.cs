using Sandcube.Blocks.Entities;
using Sandcube.Mth;

namespace Sandcube.Worlds;

public interface IBlockEntityProvider
{
    // Thread safe
    BlockEntity? GetBlockEntity(Vector3Int position);
}
