using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Data;

public interface IReadOnlyBlocksContainer
{
    BlockState? GetBlockState(Vector3Int position);
    BlockEntity? GetBlockEntity(Vector3Int position);
}
