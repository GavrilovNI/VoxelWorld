using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Data;

public class BlocksContainer
{
    public Dictionary<Vector3Int, BlockState> BlockStates = new();
    public SortedDictionary<Vector3Int, BlockEntity> BlockEntities = new(Vector3Int.XYZIterationComparer);

    public void Clear()
    {
        BlockStates.Clear();
        BlockEntities.Clear();
    }

    public BlockState? GetBlockState(Vector3Int position) => BlockStates!.GetValueOrDefault(position, null);
    public BlockEntity? GetBlockEntity(Vector3Int position) => BlockEntities!.GetValueOrDefault(position, null);

    public bool IsEmpty() => BlockStates.Count == 0 && BlockEntities.Count == 0;
}
