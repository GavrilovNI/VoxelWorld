﻿using Sandbox;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Mth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VoxelWorld.IO.Helpers;

public class RegionalSaveHelper
{
    public BaseFileSystem FileSystem { get; }
    public Vector3IntB RegionSize { get; }
    public string FilesExtension { get; }
    protected int MaxChunksCount { get; }
    protected BBoxInt Bounds { get; }

    public RegionalSaveHelper(BaseFileSystem fileSystem, Vector3IntB regionSize, string filesExtension)
    {
        FileSystem = fileSystem;
        RegionSize = regionSize;
        FilesExtension = filesExtension;
        MaxChunksCount = RegionSize.x * RegionSize.y * RegionSize.z;
        Bounds = BBoxInt.FromMinsAndSize(0, RegionSize);
    }

    public bool TryLoadOneChunkOnly(Vector3IntB globalChunkPosition, out BinaryTag chunkTag)
    {
        var regionPosition = (1f * globalChunkPosition / RegionSize).Floor();
        if(!HasRegionFile(regionPosition))
        {
            chunkTag = null!;
            return false;
        }

        ListTag regionTag;
        using(var regionReadStream = OpenRegionRead(regionPosition))
        {
            using var reader = new BinaryReader(regionReadStream);
            regionTag = BinaryTag.Read(reader).To<ListTag>();
        }

        var firstChunkPosition = regionPosition * RegionSize;
        var localChunkPosition = globalChunkPosition - firstChunkPosition;
        var chunkIndex = GetChunkIndex(localChunkPosition);

        if(chunkIndex >= regionTag.Count)
        {
            chunkTag = null!;
            return false;
        }

        chunkTag = regionTag.GetTag(chunkIndex);
        if(chunkTag.IsDataEmpty)
        {
            chunkTag = null!;
            return false;
        }
        return true;
    }

    public Dictionary<Vector3IntB, BinaryTag> LoadRegion(Vector3IntB regionPosition)
    {
        if(!HasRegionFile(regionPosition))
            return new();

        BinaryTag possibleRegionTag;
        using(var regionReadStream = OpenRegionRead(regionPosition))
        {
            using var reader = new BinaryReader(regionReadStream);
            possibleRegionTag = BinaryTag.Read(reader);
        }

        if(possibleRegionTag is not ListTag regionTag || regionTag.TagsType != BinaryTagType.Compound)
            return new();

        Dictionary<Vector3IntB, BinaryTag> result = new();
        for(int chunkIndex = 0; chunkIndex < MaxChunksCount && chunkIndex < regionTag.Count; ++chunkIndex)
        {
            var chunkPosition = GetChunkPosition(chunkIndex);
            result[chunkPosition] = regionTag[chunkIndex];
        }
        return result;
    }

    public void SaveRegion(Vector3IntB regionPosition, IReadOnlyDictionary<Vector3IntB, BinaryTag> localChunkedTags)
    {
        if(localChunkedTags.All(x => x.Value.IsDataEmpty))
        {
            DeleteRegionFile(regionPosition);
            return;
        }

        ListTag regionTag;
        if(HasRegionFile(regionPosition))
        {
            using var regionReadStream = OpenRegionRead(regionPosition);
            using var reader = new BinaryReader(regionReadStream);
            regionTag = BinaryTag.Read(reader).To<ListTag>();
        }
        else
        {
            regionTag = new();
        }

        foreach(var (chunkPosition, chunkTag) in localChunkedTags)
        {
            var chunkIndex = GetChunkIndex(chunkPosition);
            if(chunkIndex < 0 || chunkIndex >= MaxChunksCount)
                throw new ArgumentException($"{localChunkedTags} is not a local chunk position in region");
            regionTag[chunkIndex] = chunkTag;
        }

        using var regionWriteStream = OpenRegionWrite(regionPosition);
        using var writer = new BinaryWriter(regionWriteStream);
        regionTag.Write(writer);
    }

    public virtual void SaveChunks(IReadOnlyDictionary<Vector3IntB, BinaryTag> chunkedTags)
    {
        var regionedData = chunkedTags.GroupBy(c => (1f * c.Key / RegionSize).Floor());
        foreach(var region in regionedData)
        {
            var regionPosition = region.Key;
            var firstChunkPosition = regionPosition * RegionSize;
            var localTags = region.ToDictionary(kv => kv.Key - firstChunkPosition, kv => kv.Value);
            SaveRegion(regionPosition, localTags);
        }
    }


    protected virtual int GetChunkIndex(Vector3IntB chunkPosition)
    {
        return chunkPosition.z + RegionSize.z * (chunkPosition.y + RegionSize.y * chunkPosition.x);
    }

    protected virtual Vector3IntB GetChunkPosition(int chunkIndex)
    {
        int z = chunkIndex % RegionSize.z;
        chunkIndex /= RegionSize.z;
        int y = chunkIndex % RegionSize.y;
        chunkIndex /= RegionSize.y;
        int x = chunkIndex % RegionSize.x;
        return new(x, y, z);
    }

    public virtual string GetRegionFilePath(in Vector3IntB regionPosition) =>
        $"{regionPosition.x}.{regionPosition.y}.{regionPosition.z}.{FilesExtension}";

    public virtual bool HasRegionFile(in Vector3IntB regionPosition) =>
        FileSystem.FileExists(GetRegionFilePath(regionPosition));

    public virtual bool DeleteRegionFile(in Vector3IntB regionPosition)
    {
        var filePath = GetRegionFilePath(regionPosition);
        bool hadFile = FileSystem.FileExists(filePath);
        if(hadFile)
            FileSystem.DeleteFile(filePath);
        return hadFile;
    }

    public virtual Dictionary<Vector3IntB, string> GetAllRegionFiles()
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

    public virtual Stream OpenRegionRead(in Vector3IntB regionPosition, FileMode fileMode = FileMode.Open)
    {
        string fileName = GetRegionFilePath(regionPosition);
        return FileSystem.OpenRead(fileName, fileMode);
    }

    public virtual Stream OpenRegionWrite(in Vector3IntB regionPosition, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetRegionFilePath(regionPosition);
        return FileSystem.OpenWrite(fileName, fileMode);
    }
}
