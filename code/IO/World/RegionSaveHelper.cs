using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Data.Enumarating;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO.World;

public class RegionSaveHelper : IBinaryWritable, IBinaryReadable
{
    protected readonly Vector3Int RegionPosition;
    protected readonly WorldSaveOptions WorldSaveOptions;

    protected BlockStatePalette BlockStatePalette;
    protected readonly int MaxChunksCount;

    protected readonly Dictionary<Vector3Int, BlocksContainer> Chunks = new();

    protected virtual IEnumerator<Vector3Int> AllChunkPositions
    {
        get
        {
            var regionSize = WorldSaveOptions.RegionSize;
            for(int x = 0; x < regionSize.x; ++x)
            {
                for(int y = 0; y < regionSize.y; ++y)
                {
                    for(int z = 0; z < regionSize.z; ++z)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }

    protected virtual IEnumerator<Vector3Int> AllBlockPositionsInChunk
    {
        get
        {
            var chunkSize = WorldSaveOptions.ChunkSize;
            for(int x = 0; x < chunkSize.x; ++x)
            {
                for(int y = 0; y < chunkSize.y; ++y)
                {
                    for(int z = 0; z < chunkSize.z; ++z)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }

    public RegionSaveHelper(in WorldSaveOptions worldSaveOptions, in Vector3Int regionPosition)
    {
        WorldSaveOptions = worldSaveOptions;
        RegionPosition = regionPosition;

        var regionSize = WorldSaveOptions.RegionSize;
        MaxChunksCount = regionSize.x * regionSize.y * regionSize.z;

        BlockStatePalette = new BlockStatePalette();
        UpdateBlockStatePalette();
    }

    public virtual void Clear()
    {
        Chunks.Clear();
        UpdateBlockStatePalette();
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

    public virtual bool ReadOnlyOneChunk(BinaryReader reader, Vector3Int chunkPosition, bool readToEnd = true)
    {
        BlockStatePalette = BlockStatePalette.Read(reader);
        return ReadChunk(reader, chunkPosition, readToEnd);
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

        for(int i = 0; i < MaxChunksCount; ++i)
        {
            var chunkOffset = chunkOffsets[i];
            if(chunkOffset < 0)
                continue;

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
        if(chunkOffset < 0)
            return false;

        Chunks[chunkPosition] = ReadChunkData(reader);

        if(readToEnd)
            reader.BaseStream.Position = chunksEnd;

        return true;
    }

    protected virtual long[] ReadChunkOffsets(BinaryReader reader)
    {
        long[] chunkOffsets = new long[MaxChunksCount];
        for(int i = 0; i <= MaxChunksCount; ++i)
            chunkOffsets[i] = reader.ReadInt64();
        return chunkOffsets;
    }

    protected virtual BlocksContainer ReadChunkData(BinaryReader reader)
    {
        var blocks = new BlocksContainer();

        foreach(var blockPosition in AllBlockPositionsInChunk)
        {
            var blockStateId = reader.ReadInt32();
            var blockState = BlockStatePalette!.GetValue(blockStateId);
            blocks.BlockStates[blockPosition] = blockState;
        }

        //TODO: read block entities

        return blocks;
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
            WriteChunkData(writer, chunkPosition);
            using(StreamPositionRememberer rememberer = writer)
            {
                int chunkIndex = GetChunkIndex(chunkPosition);
                long chunkOffset = rememberer.StartPosition - chunksStart;
                writer.BaseStream.Position = chunksOffsetsStart + chunkIndex * sizeof(long);
                writer.Write(chunkOffset);
            }
        }

        using(StreamPositionRememberer rememberer = writer)
        {
            writer.BaseStream.Position = chunksOffsetsStart + MaxChunksCount * sizeof(long);
            writer.Write(rememberer.StartPosition);
        }
    }

    protected virtual void WriteChunkData(BinaryWriter writer, Vector3Int chunkPosition)
    {
        var chunk = Chunks!.GetValueOrDefault(chunkPosition, null);
        foreach(var blockPosition in AllBlockPositionsInChunk)
        {
            var blockState = chunk!.BlockStates[blockPosition];
            var blockStateId = BlockStatePalette!.GetId(blockState);
            writer.Write(blockStateId);
        }

        //TODO: write block entities
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
