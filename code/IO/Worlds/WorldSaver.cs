﻿using Sandbox;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.IO;
using System.Linq;

namespace Sandcube.IO.Worlds;

public class WorldSaver : Component, ISavePathInitializable
{
    [Property] private World World { get; set; } = null!;
    [Property] private Vector3Int RegionSize { get; set; } = Vector3Int.One * 5;
    [Property] private string SavePath { get; set; } = string.Empty;
    [Property] private bool ClickToSave { get; set; } = false;


    protected override void OnUpdate()
    {
        if(ClickToSave)
        {
            ClickToSave = false;
            Save();
        }
    }

    public virtual void InitizlizeSavePath(string savePath) => SavePath = savePath;

    public virtual void Save() => Save(FileSystem.Data.CreateDirectoryAndSubSystem(SavePath));

    protected virtual void Save(BaseFileSystem fileSystem)
    {
        var helper = new WorldSaveHelper(fileSystem);
        if(helper.TryReadWorldOptions(out var options))
        {
            if(options.ChunkSize != World.ChunkSize)
                throw new InvalidOperationException($"Can't save world, saved chunk size {options.ChunkSize} is not equal world's chunk size {World.ChunkSize}");
        }
        else
        {
            options = new WorldSaveOptions()
            {
                RegionSize = RegionSize,
                ChunkSize = World.ChunkSize,
            };
            helper.SaveWorldOptions(options);
        }

        SaveMarker saveMarker = SaveMarker.NewNotSaved;
        var chunksData = World.Save(saveMarker);
        var regions = chunksData.GroupBy(c => (1f * c.Key / RegionSize).Floor());

        foreach(var region in regions)
        {
            var regionPosition = region.Key;
            var regionHelper = new RegionSaveHelper(options);
            if(helper.HasRegionFile(regionPosition))
            {
                using(var regionReadStream = helper.OpenRegionRead(regionPosition))
                {
                    using var reader = new BinaryReader(regionReadStream);
                    regionHelper.Read(reader);
                }
            }

            var firstChunkPosition = regionPosition * RegionSize;
            foreach(var (chunkPosition, chunkData) in region)
            {
                var chunkLocalPosition = chunkPosition - firstChunkPosition;
                regionHelper.SetChunkData(chunkLocalPosition, chunkData);
            }

            using var regionWriteStream = helper.OpenRegionWrite(regionPosition);
            using var writer = new BinaryWriter(regionWriteStream);
            regionHelper.Write(writer);
        }

        saveMarker.MarkSaved();
    }
}