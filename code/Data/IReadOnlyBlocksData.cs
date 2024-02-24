using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Data;

public interface IReadOnlyBlocksData
{
    bool IsEmpty();
    BlockState? GetBlockState(in Vector3Int position);
    void UpdateEntity(in Vector3Int position, BlockEntity blockEntity);
}
