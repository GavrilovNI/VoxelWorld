using Sandcube.Mth;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO.Helpers;

public abstract class RegionalChunkedSaveHelper<T> : IBinaryWritable, IBinaryReadable where T : class
{
    public Vector3Int RegionSize { get; }

    protected readonly int MaxChunksCount;
    protected readonly BBoxInt Bounds;
    protected readonly Dictionary<Vector3Int, T> Chunks = new();

    public RegionalChunkedSaveHelper(Vector3Int regionSize)
    {
        RegionSize = regionSize;
        MaxChunksCount = RegionSize.x * RegionSize.y * RegionSize.z;
        Bounds = BBoxInt.FromMinsAndSize(0, RegionSize);
    }


    public virtual void Clear()
    {
        Chunks.Clear();
    }

    public virtual void SetChunkData(Vector3Int localChunkPosition, T blocksData)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        Chunks[localChunkPosition] = blocksData;
    }

    public virtual bool RemoveChunkData(Vector3Int localChunkPosition) =>
        Chunks.Remove(localChunkPosition);

    public virtual T? GetChunkData(Vector3Int localChunkPosition)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        return Chunks!.GetValueOrDefault(localChunkPosition, null);
    }


    #region read

    protected virtual void ReadAdditionalData(BinaryReader reader) { }

    void IBinaryReadable.Read(BinaryReader reader) => Read(reader, true);
    public virtual void Read(BinaryReader reader, bool readToEnd = true)
    {
        ReadAdditionalData(reader);
        ReadChunks(reader, readToEnd);
    }

    public virtual bool ReadOnlyOneChunk(BinaryReader reader, Vector3Int localChunkPosition, bool readToEnd = true)
    {
        if(!Bounds.Contains(localChunkPosition))
            throw new ArgumentOutOfRangeException(nameof(localChunkPosition));

        ReadAdditionalData(reader);
        return ReadChunk(reader, localChunkPosition, readToEnd);
    }

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

    protected abstract T ReadChunkData(BinaryReader reader);

    #endregion

    #region write

    protected virtual void WriteAdditionalData(BinaryWriter writer) { }

    public virtual void Write(BinaryWriter writer)
    {
        WriteAdditionalData(writer);
        WriteChunks(writer);
    }

    protected virtual void WriteChunks(BinaryWriter writer)
    {
        long chunksOffsetsStart = writer.BaseStream.Position;

        for(int i = 0; i < MaxChunksCount; ++i)
            writer.Write(-1L);
        writer.Write(-1L); // writing end chunks offset

        long chunksStart = writer.BaseStream.Position;

        foreach(var (chunkPosition, chunkData) in Chunks)
        {
            using(StreamPositionRememberer rememberer = writer)
            {
                int chunkIndex = GetChunkIndex(chunkPosition);
                long chunkOffset = rememberer.StartPosition - chunksStart;
                writer.BaseStream.Position = chunksOffsetsStart + chunkIndex * sizeof(long);
                writer.Write(chunkOffset);
            }
            WriteChunkData(writer, chunkData);
        }

        using(StreamPositionRememberer rememberer = writer)
        {
            writer.BaseStream.Position = chunksOffsetsStart + MaxChunksCount * sizeof(long);
            writer.Write(rememberer.StartPosition);
        }
    }

    protected abstract void WriteChunkData(BinaryWriter writer, T data);

    #endregion


    protected virtual int GetChunkIndex(Vector3Int chunkPosition)
    {
        return chunkPosition.z + RegionSize.z * (chunkPosition.y + chunkPosition.x * RegionSize.y);
    }

    protected virtual Vector3Int GetChunkPosition(int chunkIndex)
    {
        int z = chunkIndex % RegionSize.z;
        chunkIndex /= RegionSize.z;
        int y = chunkIndex % RegionSize.y;
        chunkIndex /= RegionSize.y;
        int x = chunkIndex % RegionSize.x;
        return new(x, y, z);
    }
}
