using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Mth;
using System.IO;
using System.Collections.Generic;

namespace Sandcube.IO.Helpers;

public class RegionSaveHelper : RegionalChunkedSaveHelper<BlocksData>
{
    protected readonly WorldOptions WorldOptions;

    protected BlockStatePalette BlockStatePalette;

    protected virtual IEnumerable<Vector3Int> AllBlockPositionsInChunk => WorldOptions.ChunkSize.GetPositionsFromZero(false);
    
    public RegionSaveHelper(in WorldOptions worldOptions) : base(worldOptions.RegionSize)
    {
        WorldOptions = worldOptions;
        BlockStatePalette = new BlockStatePalette();
        UpdateBlockStatePalette();
    }

    public override void Clear()
    {
        base.Clear();
        UpdateBlockStatePalette();
    }

    protected override void WriteAdditionalData(BinaryWriter writer)
    {
        UpdateBlockStatePalette();
        writer.Write(BlockStatePalette);
    }

    protected override void ReadAdditionalData(BinaryReader reader)
    {
        BlockStatePalette = BlockStatePalette.Read(reader);
    }

    protected virtual void UpdateBlockStatePalette()
    {
        BlockStatePalette.Clear();
        BlockStatePalette.GetOrAdd(BlockState.Air);

        foreach(var (_, chunk) in Chunks)
        {
            foreach(var (_, blockState) in chunk.BlockStates)
            {
                BlockStatePalette.GetOrAdd(blockState);
            }
        }
    }

    protected override BlocksData ReadChunkData(BinaryReader reader)
    {
        var data = new BlocksData();

        foreach(var blockPosition in AllBlockPositionsInChunk)
        {
            var blockStateId = reader.ReadInt32();
            var blockState = BlockStatePalette!.GetValue(blockStateId);
            data.BlockStates[blockPosition] = blockState;
        }

        int blockEntitiesCount = reader.ReadInt32();
        for(int i = 0; i < blockEntitiesCount; ++i)
        {
            var blockPosition = Vector3Int.Read(reader);
            int blockEntityDataLength = reader.ReadInt32();
            if(blockEntityDataLength > 0)
            {
                var blockEntityData = reader.ReadBytes(blockEntityDataLength);
                data.BlockEntitiesData[blockPosition] = blockEntityData;
            }
        }

        return data;
    }

    protected override void WriteChunkData(BinaryWriter writer, BlocksData data)
    {
        foreach(var blockPosition in AllBlockPositionsInChunk)
        {
            var blockState = data.BlockStates.GetValueOrDefault(blockPosition, BlockState.Air);
            var blockStateId = BlockStatePalette!.GetId(blockState);
            writer.Write(blockStateId);
        }

        writer.Write(data.BlockEntitiesData.Count);
        foreach(var (blockPosition, blockEntityData) in data.BlockEntitiesData)
        {
            if(blockEntityData.Length > 0)
            {
                writer.Write(blockPosition);
                writer.Write(blockEntityData.Length);
                writer.Write(blockEntityData);
            }
        }
    }
}
