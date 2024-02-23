using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Data;

public class BlocksData
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
        IReadOnlyDictionary<Vector3Int, BlockEntity> blockEntities, bool keepEntitiesDirty = false)
    {
        BlockStates = new(blockStates);
        BlockEntitiesData = new();

        foreach(var (position, blockkEntity) in blockEntities)
            SetEntityData(position, blockkEntity, keepEntitiesDirty);
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

    public void SetEntityData(in Vector3Int position, in BlockEntity blockEntity, bool keepDirty = false)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        blockEntity.Save(writer, keepDirty);

        if(stream.Length == 0)
            return;

        var data = stream.ToArray();
        BlockEntitiesData[position] = data;
    }

    public void UpdateEntity(BlockEntity blockEntity, bool setDirty = false) => UpdateEntity(blockEntity.Position, blockEntity, setDirty);
    public void UpdateEntity(in Vector3Int position, BlockEntity blockEntity, bool setDirty = false)
    {
        if(!BlockEntitiesData.TryGetValue(position, out var data))
            return;

        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        blockEntity.Load(reader, setDirty);
    }
}