using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Data.Enumarating;
using Sandcube.Mth;
using System;
using System.IO;
using System.Collections.Generic;

namespace Sandcube.IO.Worlds;

public class RegionSaveHelper : IBinaryWritable, IBinaryReadable
{
    protected readonly WorldSaveOptions WorldSaveOptions;

    protected BlockStatePalette BlockStatePalette;
    protected readonly int MaxChunksCount;
    protected readonly BBoxInt Bounds;

    protected readonly Dictionary<Vector3Int, BlocksData> Chunks = new();

    protected virtual IEnumerator<Vector3Int> AllBlockPositionsInChunk => WorldSaveOptions.ChunkSize.GetPositionsFromZero(false);

    public RegionSaveHelper(in WorldSaveOptions worldSaveOptions)
    {
        WorldSaveOptions = worldSaveOptions;

        var regionSize = WorldSaveOptions.RegionSize;
        MaxChunksCount = regionSize.x * regionSize.y * regionSize.z;
        Bounds = BBoxInt.FromMinsAndSize(0, regionSize);

        BlockStatePalette = new BlockStatePalette();
        UpdateBlockStatePalette();
    }

    public virtual void Clear()
    {
        Chunks.Clear();
        UpdateBlockStatePalette();
    }

    public virtual void SetChunkData(Vector3Int localChunkPosition, BlocksData blocksData)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        Chunks[localChunkPosition] = blocksData;
    }

    public virtual bool RemoveChunksData(Vector3Int localChunkPosition, BlocksData blocksData) =>
        Chunks.Remove(localChunkPosition);

    public virtual BlocksData? GetChunkData(Vector3Int localChunkPosition)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        return Chunks!.GetValueOrDefault(localChunkPosition, null);
    }

    public virtual void Write(BinaryWriter writer)
    {
        UpdateBlockStatePalette();
        writer.Write(BlockStatePalette);
        WriteChunks(writer);
    }

    void IBinaryReadable.Read(BinaryReader reader) => Read(reader, true);
    public virtual void Read(BinaryReader reader, bool readToEnd = true)
    {
        BlockStatePalette = BlockStatePalette.Read(reader);
        ReadChunks(reader, readToEnd);
    }

    public virtual bool ReadOnlyOneChunk(BinaryReader reader, Vector3Int localChunkPosition, bool readToEnd = true)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        BlockStatePalette = BlockStatePalette.Read(reader);
        return ReadChunk(reader, localChunkPosition, readToEnd);
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

    #region read

    protected virtual void ReadChunks(BinaryReader reader, bool readToEnd = true)
    {
        long[] chunkOffsets = ReadChunkOffsets(reader);
        long chunksEnd = reader.ReadInt64();

        var chunksStart = reader.BaseStream.Position;
        for(int i = 0; i < MaxChunksCount; ++i)
        {
            var chunkOffset = chunkOffsets[i];
            if(chunkOffset < 0)
                continue;

            reader.BaseStream.Position = chunksStart + chunkOffset;
            Vector3Int chunkPosition = GetChunkPosition(i);
            Chunks[chunkPosition] = ReadChunkData(reader);
        }

        if(readToEnd)
            reader.BaseStream.Position = chunksEnd;
    }

    protected virtual bool ReadChunk(BinaryReader reader, Vector3Int chunkPosition, bool readToEnd = true)
    {
        long[] chunkOffsets = ReadChunkOffsets(reader);
        long chunksEnd = reader.ReadInt64();

        int index = GetChunkIndex(chunkPosition);
        var chunkOffset = chunkOffsets[index];
        if(chunkOffset >= 0)
        {
            reader.BaseStream.Position += chunkOffset;
            Chunks[chunkPosition] = ReadChunkData(reader);
        }

        if(readToEnd)
            reader.BaseStream.Position = chunksEnd;

        return true;
    }

    protected virtual long[] ReadChunkOffsets(BinaryReader reader)
    {
        long[] chunkOffsets = new long[MaxChunksCount];
        for(int i = 0; i < MaxChunksCount; ++i)
            chunkOffsets[i] = reader.ReadInt64();
        return chunkOffsets;
    }

    protected virtual BlocksData ReadChunkData(BinaryReader reader)
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
            int blockPosition = reader.ReadInt32();
            int blockEntityDataLength = reader.ReadInt32();
            if(blockEntityDataLength > 0)
            {
                var blockEntityData = reader.ReadBytes(blockEntityDataLength);
                data.BlockEntitiesData[blockPosition] = blockEntityData;
            }
        }

        return data;
    }

    #endregion

    #region write

    protected virtual void WriteChunks(BinaryWriter writer)
    {
        long chunksOffsetsStart = writer.BaseStream.Position;

        for(int i = 0; i < MaxChunksCount; ++i)
            writer.Write(-1L);
        writer.Write(-1L); // writing end chunks offset

        long chunksStart = writer.BaseStream.Position;

        foreach(var (chunkPosition, chunk) in Chunks)
        {
            using(StreamPositionRememberer rememberer = writer)
            {
                int chunkIndex = GetChunkIndex(chunkPosition);
                long chunkOffset = rememberer.StartPosition - chunksStart;
                writer.BaseStream.Position = chunksOffsetsStart + chunkIndex * sizeof(long);
                writer.Write(chunkOffset);
            }
            WriteChunkData(writer, chunkPosition);
        }

        using(StreamPositionRememberer rememberer = writer)
        {
            writer.BaseStream.Position = chunksOffsetsStart + MaxChunksCount * sizeof(long);
            writer.Write(rememberer.StartPosition);
        }
    }

    protected virtual void WriteChunkData(BinaryWriter writer, Vector3Int chunkPosition)
    {
        var chunk = Chunks[chunkPosition];

        foreach(var blockPosition in AllBlockPositionsInChunk)
        {
            var blockState = chunk.BlockStates.GetValueOrDefault(blockPosition, BlockState.Air);
            var blockStateId = BlockStatePalette!.GetId(blockState);
            writer.Write(blockStateId);
        }

        writer.Write(chunk.BlockEntitiesData.Count);
        foreach(var (blockPosition, blockEntityData) in chunk.BlockEntitiesData)
        {
            if(blockEntityData.Length > 0)
            {
                writer.Write(blockPosition);
                writer.Write(blockEntityData.Length);
                writer.Write(blockEntityData);
            }
        }
    }

    #endregion


    protected virtual int GetChunkIndex(Vector3Int chunkPosition)
    {
        var regionSize = WorldSaveOptions.RegionSize;
        return chunkPosition.z + regionSize.z * (chunkPosition.y + chunkPosition.x * regionSize.y);
    }

    protected virtual Vector3Int GetChunkPosition(int chunkIndex)
    {
        var regionSize = WorldSaveOptions.RegionSize;
        int z = chunkIndex % regionSize.z;
        chunkIndex /= regionSize.z;
        int y = chunkIndex % regionSize.y;
        chunkIndex /= regionSize.y;
        int x = chunkIndex % regionSize.x;
        return new(x, y, z);
    }
}
