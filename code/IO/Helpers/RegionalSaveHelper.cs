using Sandbox;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VoxelWorld.IO.Helpers;

public sealed class RegionalSaveHelper
{
    public const int Version = 1;

    public BaseFileSystem FileSystem { get; }
    public Vector3Byte RegionSize { get; }
    public string FilesExtension { get; }
    private readonly int _maxChunksCount;

    public RegionalSaveHelper(BaseFileSystem fileSystem, Vector3Byte regionSize, string filesExtension)
    {
        FileSystem = fileSystem;
        RegionSize = regionSize;
        FilesExtension = filesExtension;
        _maxChunksCount = RegionSize.x * RegionSize.y * RegionSize.z;
    }

    public bool TryLoadOneChunkOnly(in Vector3IntB globalChunkPosition, out BinaryTag chunkTag)
    {
        var regionPosition = GetRegionPosition(in globalChunkPosition);
        if(!HasRegionFile(in regionPosition))
        {
            chunkTag = null!;
            return false;
        }

        using var regionReadStream = OpenRegionRead(in regionPosition);
        using var reader = new BinaryReader(regionReadStream);

        Header header = new(Version, _maxChunksCount);
        header.ReadInfo(reader);

        var chunkPosition = GetLocalChunkPosition(in regionPosition, in globalChunkPosition);
        var chunkIndex = GetChunkIndex(in chunkPosition);
        header.ReadChunkOffset(reader, chunkIndex);

        if(!header.ChunkExists(chunkIndex))
        {
            chunkTag = null!;
            return false;
        }

        header.SeekToChunk(reader, chunkIndex);
        chunkTag = BinaryTag.Read(reader);
        return true;
    }

    public Dictionary<Vector3Byte, BinaryTag> LoadRegion(in Vector3IntB regionPosition) => LoadRegion(in regionPosition, new HashSet<Vector3Byte>());
    public Dictionary<Vector3Byte, BinaryTag> LoadRegion(in Vector3IntB regionPosition, ISet<Vector3Byte> excludingChunkLocalPositions)
    {
        if(!HasRegionFile(in regionPosition))
            return new();

        using var regionReadStream = OpenRegionRead(in regionPosition);
        using var reader = new BinaryReader(regionReadStream);

        Header header = Header.Read(reader, Version, _maxChunksCount);

        Dictionary<Vector3Byte, BinaryTag> result = new();
        for(int i = 0; i < _maxChunksCount; ++i)
        {
            var chunkPosition = GetChunkPosition(i);
            if(header.ChunkExists(i) && !excludingChunkLocalPositions.Contains(chunkPosition))
            {
                header.SeekToChunk(reader, i);
                var chunkTag = BinaryTag.Read(reader);
                result[chunkPosition] = chunkTag;
            }
        }

        return result;
    }

    public void SaveRegion(in Vector3IntB regionPosition, IReadOnlyDictionary<Vector3Byte, BinaryTag> localChunkedTags)
    {
        if(localChunkedTags.All(x => x.Value.IsDataEmpty))
        {
            DeleteRegionFile(in regionPosition);
            return;
        }

        Dictionary<Vector3Byte, BinaryTag> savedChunks = LoadRegion(in regionPosition, localChunkedTags.Keys.ToHashSet());

        using var regionWriteStream = OpenRegionWrite(in regionPosition);
        using var writer = new BinaryWriter(regionWriteStream);

        Header header = new(Version, _maxChunksCount);
        header.Write(writer);

        foreach(var (chunkPosition, chunkTag) in savedChunks.Concat(localChunkedTags))
        {
            var chunkIndex = GetChunkIndex(in chunkPosition);
            if(chunkIndex < 0 || chunkIndex >= _maxChunksCount)
                throw new ArgumentException($"{localChunkedTags} is not a local chunk position in region");

            using(StreamPositionRememberer chunkStartRememberer = writer)
            {
                header.SetChunkOffset(chunkIndex, chunkStartRememberer.StartPosition);
                header.WriteChunkOffset(writer, chunkIndex);
            }

            chunkTag.Write(writer);
        }

        header.EndOffset = writer.BaseStream.Position;
        header.WriteEndOffset(writer);
    }

    public void SaveChunks(IReadOnlyDictionary<Vector3IntB, BinaryTag> chunkedTags)
    {
        var regionedData = chunkedTags.GroupBy(c => GetRegionPosition(c.Key));
        foreach(var region in regionedData)
        {
            var regionPosition = region.Key;
            var localTags = region.ToDictionary(kv => GetLocalChunkPosition(in regionPosition, kv.Key), kv => kv.Value);
            SaveRegion(regionPosition, localTags);
        }
    }


    private int GetChunkIndex(in Vector3Byte chunkPosition)
    {
        return chunkPosition.z + RegionSize.z * (chunkPosition.y + RegionSize.y * chunkPosition.x);
    }

    private Vector3Byte GetChunkPosition(int chunkIndex)
    {
        int z = chunkIndex % RegionSize.z;
        chunkIndex /= RegionSize.z;
        int y = chunkIndex % RegionSize.y;
        chunkIndex /= RegionSize.y;
        int x = chunkIndex % RegionSize.x;
        return new((byte)x, (byte)y, (byte)z);
    }

    private Vector3IntB GetRegionPosition(in Vector3IntB globalChunkPosition) => (1f * globalChunkPosition / RegionSize).Floor();

    private Vector3Byte GetLocalChunkPosition(in Vector3IntB regionPosition, in Vector3IntB globalChunkPosition)
    {
        var firstChunkPosition = regionPosition * RegionSize;
        return (Vector3Byte)(globalChunkPosition - firstChunkPosition);
    }

    public string GetRegionFilePath(in Vector3IntB regionPosition) =>
        $"{regionPosition.x}.{regionPosition.y}.{regionPosition.z}.{FilesExtension}";

    public bool HasRegionFile(in Vector3IntB regionPosition) =>
        FileSystem.FileExists(GetRegionFilePath(regionPosition));

    public bool DeleteRegionFile(in Vector3IntB regionPosition)
    {
        var filePath = GetRegionFilePath(regionPosition);
        bool hadFile = FileSystem.FileExists(filePath);
        if(hadFile)
            FileSystem.DeleteFile(filePath);
        return hadFile;
    }

    public Dictionary<Vector3IntB, string> GetAllRegionFiles()
    {
        var posibleFiles = FileSystem.FindFile("/", $"*.*.*.{FilesExtension}", false);

        Dictionary<Vector3IntB, string> result = new();
        foreach(var posibleFile in posibleFiles)
        {
            var parts = posibleFile.Split('.', 4);
            if(parts.Length != 4)
                continue;

            if(!int.TryParse(parts[0], out int x) ||
                !int.TryParse(parts[1], out int y) ||
                !int.TryParse(parts[2], out int z))
                continue;

            result.Add(new(x, y, z), posibleFile);
        }
        return result;
    }

    public Stream OpenRegionRead(in Vector3IntB regionPosition, FileMode fileMode = FileMode.Open)
    {
        string fileName = GetRegionFilePath(regionPosition);
        return FileSystem.OpenRead(fileName, fileMode);
    }

    public Stream OpenRegionWrite(in Vector3IntB regionPosition, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetRegionFilePath(regionPosition);
        return FileSystem.OpenWrite(fileName, fileMode);
    }


    private struct Header
    {
        public int Version { get; set; }
        public int MaxChunksCount { get; set; }
        public long OffsetsStart { get; set; } = -1L;
        private long[]? _chunkOffsets = null;
        public long EndOffset { get; set; } = -1L;

        public Header() : this(-1, -1)
        {
        }

        public Header(int requiredVersion, int requiredMaxChunksCount)
        {
            Version = requiredVersion;
            MaxChunksCount = requiredMaxChunksCount;
        }

        private long[]? GetOrCreateChunkOffsets(int minLength)
        {
            int maxChunksCount = MaxChunksCount;
            if(minLength < 0 || minLength > maxChunksCount)
                throw new ArgumentOutOfRangeException(nameof(minLength), minLength, $"{nameof(minLength)} is not in range [0, {nameof(MaxChunksCount)}) [0, {maxChunksCount}).");

            if(_chunkOffsets is null || _chunkOffsets.Length <= minLength)
            {
                var oldChunkOffsets = _chunkOffsets;
                _chunkOffsets = new long[maxChunksCount];

                int copyCount = 0;
                if(oldChunkOffsets is not null)
                {
                    copyCount = oldChunkOffsets.Length;
                    Array.Copy(oldChunkOffsets, 0, _chunkOffsets, 0, copyCount);
                }

                for(int i = copyCount; i < maxChunksCount; ++i)
                    _chunkOffsets[i] = -1L;
            }
            return _chunkOffsets;
        }

        public readonly long GetChunkOffset(int chunkIndex)
        {
            if(chunkIndex < 0 || chunkIndex > MaxChunksCount)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex), chunkIndex, $"Chunk index is out of range [0, {MaxChunksCount}).");
            
            if(_chunkOffsets is null || _chunkOffsets.Length <= chunkIndex)
                return -1;

            return _chunkOffsets[chunkIndex];
        }

        public void SetChunkOffset(int chunkIndex, long chunkOffset)
        {
            int maxChunksCount = MaxChunksCount;
            if(chunkIndex < 0 || chunkIndex > maxChunksCount)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex), chunkIndex, $"Chunk index is out of range [0, {MaxChunksCount}).");

            GetOrCreateChunkOffsets(chunkIndex)![chunkIndex] = chunkOffset;
        }

        public static Header Read(BinaryReader reader, int requiredVersion, int requiredMaxChunksCount)
        {
            Header header = new(requiredVersion, requiredMaxChunksCount);
            header.Read(reader);
            return header;
        }


        public readonly bool ChunkExists(int chunkIndex) => GetChunkOffset(chunkIndex) >= 0;


        public readonly void SeekToChunk(Stream stream, int chunkIndex)
        {
            var offset = GetChunkOffset(chunkIndex);
            if(offset < 0)
                throw new InvalidOperationException("Chunk doesn't exist.");
            
            stream.Position = offset;
        }
        public readonly void SeekToChunk(BinaryReader reader, int chunkIndex) => SeekToChunk(reader.BaseStream, chunkIndex);
        public readonly void SeekToChunk(BinaryWriter writer, int chunkIndex) => SeekToChunk(writer.BaseStream, chunkIndex);


        public void ReadInfo(BinaryReader reader)
        {
            var version = reader.ReadInt32();
            if(version <= 0)
                throw new InvalidOperationException($"Read not positive version({version}).");

            if(Version > 0 && version != Version)
                throw new InvalidOperationException($"Read region version({version}) is not the same as current one({Version}).");
            Version = version;

            var maxChunksCount = reader.ReadInt32();
            if(maxChunksCount <= 0)
                throw new InvalidOperationException($"Read not positive maxChunksCount({maxChunksCount}).");

            if(MaxChunksCount > 0)
            {
                if(maxChunksCount != MaxChunksCount)
                    throw new InvalidOperationException($"Read region maxChunksCount({maxChunksCount}) is not the same as current one({MaxChunksCount}).");
            }
            MaxChunksCount = maxChunksCount;

            OffsetsStart = reader.BaseStream.Position;
        }

        public void ReadOffsets(BinaryReader reader)
        {
            reader.BaseStream.Position = OffsetsStart;

            var maxChunksCount = MaxChunksCount;
            _chunkOffsets = new long[maxChunksCount];
            for(int i = 0; i < maxChunksCount; ++i)
                _chunkOffsets[i] = reader.ReadInt64();

            EndOffset = reader.ReadInt64();
        }

        public long ReadChunkOffset(BinaryReader reader, int chunkIndex)
        {
            reader.BaseStream.Position = OffsetsStart + sizeof(long) * chunkIndex;
            var offset = reader.ReadInt64();
            SetChunkOffset(chunkIndex, offset);
            return offset;
        }

        public long ReadEndOffset(BinaryReader reader)
        {
            reader.BaseStream.Position = OffsetsStart + sizeof(long) * MaxChunksCount;
            var offset = reader.ReadInt64();
            EndOffset = offset;
            return offset;
        }

        public void Read(BinaryReader reader)
        {
            ReadInfo(reader);
            ReadOffsets(reader);
        }



        public void WriteInfo(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(MaxChunksCount);
            OffsetsStart = writer.BaseStream.Position;
        }

        public readonly void WriteOffsets(BinaryWriter writer)
        {
            writer.BaseStream.Position = OffsetsStart;

            var maxChunksCount = MaxChunksCount;
            for(int i = 0; i < maxChunksCount; ++i)
                writer.Write(GetChunkOffset(i));

            writer.Write(EndOffset);
        }

        public readonly void WriteChunkOffset(BinaryWriter writer, int chunkIndex)
        {
            writer.BaseStream.Position = OffsetsStart + sizeof(long) * chunkIndex;
            writer.Write(GetChunkOffset(chunkIndex));
        }

        public readonly void WriteEndOffset(BinaryWriter writer)
        {
            writer.BaseStream.Position = OffsetsStart + sizeof(long) * MaxChunksCount;
            writer.Write(EndOffset);
        }

        public void Write(BinaryWriter writer)
        {
            WriteInfo(writer);
            WriteOffsets(writer);
        }
    }
}
