using Sandbox;
using Sandcube.Data;
using Sandcube.IO.Helpers;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.IO.Worlds;

public class WorldSaver : Component, ISaver
{
    [Property] private World World { get; set; } = null!;

    protected BaseFileSystem WorldFileSystem => World.WorldFileSystem!;
    protected WorldOptions WorldOptions => World.WorldOptions;


    public virtual async Task<bool> Save()
    {
        if(WorldOptions.ChunkSize != World.ChunkSize)
            throw new InvalidOperationException($"Can't save world, saved chunk size {WorldOptions.ChunkSize} is not equal world's chunk size {World.ChunkSize}");

        SaveMarker saveMarker = SaveMarker.NewNotSaved;
        var worldData = World.Save(saveMarker);

        var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);

        var blocksTask = SaveBlocks(worldSaveHelper, worldData.Chunks);

        var results = await Task.WhenAll(blocksTask);

        bool saved = results.All(saved => saved);
        
        if(saved)
            saveMarker.MarkSaved();

        return saved;
    }

    protected virtual Task<bool> SaveBlocks(WorldSaveHelper worldSaveHelper, IReadOnlyDictionary<Vector3Int, BlocksData> chunksData)
    {
        var regions = chunksData.GroupBy(c => (1f * c.Key / WorldOptions.RegionSize).Floor());

        var regionSaveHelper = new RegionSaveHelper(WorldOptions);
        foreach(var region in regions)
        {
            var regionPosition = region.Key;
            if(worldSaveHelper.HasRegionFile(regionPosition))
            {
                using(var regionReadStream = worldSaveHelper.OpenRegionRead(regionPosition))
                {
                    using var reader = new BinaryReader(regionReadStream);
                    regionSaveHelper.Read(reader);
                }
            }

            var firstChunkPosition = regionPosition * WorldOptions.RegionSize;
            foreach(var (chunkPosition, chunkData) in region)
            {
                var chunkLocalPosition = chunkPosition - firstChunkPosition;
                regionSaveHelper.SetChunkData(chunkLocalPosition, chunkData);
            }

            using var regionWriteStream = worldSaveHelper.OpenRegionWrite(regionPosition);
            using var writer = new BinaryWriter(regionWriteStream);
            regionSaveHelper.Write(writer);
        }
        return Task.FromResult(true);
    }
}
