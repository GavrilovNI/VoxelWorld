using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Data;

public class BlocksData : IReadOnlyBlocksData
{
    public Dictionary<Vector3Int, BlockState> BlockStates;
    public Dictionary<Vector3Int, byte[]> BlockEntitiesData;

    public BlocksData()
    {
        BlockStates = new();
        BlockEntitiesData = new();
    }

    public BlocksData(IReadOnlyDictionary<Vector3Int, BlockState> blockStates)
    {
        BlockStates = new(blockStates);
        BlockEntitiesData = new();
    }

    public BlocksData(IReadOnlyDictionary<Vector3Int, BlockState> blockStates,
        IReadOnlyDictionary<Vector3Int, BlockEntity> blockEntities)
    {
        BlockStates = new(blockStates);
        BlockEntitiesData = new();

        foreach(var (position, blockkEntity) in blockEntities)
            SetEntityData(position, blockkEntity);
    }

    public BlocksData(BlocksData other)
    {
        BlockStates = new(other.BlockStates);
        BlockEntitiesData = new(other.BlockEntitiesData);
    }

    public void Clear()
    {
        BlockStates.Clear();
        BlockEntitiesData.Clear();
    }

    public bool IsEmpty() => BlockStates.Count == 0 && BlockEntitiesData.Count == 0;

    public BlockState? GetBlockState(in Vector3Int position) => BlockStates!.GetValueOrDefault(position, null);

    public void SetEntityData(in Vector3Int position, in BlockEntity blockEntity)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        blockEntity.Write(writer);

        if(stream.Length == 0)
            return;

        var data = stream.ToArray();
        BlockEntitiesData[position] = data;
    }

    public void UpdateEntity(in Vector3Int position, BlockEntity blockEntity)
    {
        if(!BlockEntitiesData.TryGetValue(position, out var data))
            return;

        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        blockEntity.Load(reader);
    }
}