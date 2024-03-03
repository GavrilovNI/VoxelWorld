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

    protected GameSaveHelper GameSaveHelper => SandcubeGame.Instance!.CurrentGameSaveHelper!;
    protected BaseFileSystem WorldFileSystem => World.WorldFileSystem!;
    protected WorldOptions WorldOptions => World.WorldOptions;


    public virtual async Task<bool> Save()
    {
        if(WorldOptions.ChunkSize != World.ChunkSize)
            throw new InvalidOperationException($"Can't save world, saved chunk size {WorldOptions.ChunkSize} is not equal world's chunk size {World.ChunkSize}");

        SaveMarker saveMarker = SaveMarker.NewNotSaved;
        var worldData = World.Save(saveMarker);

        var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);

        var blocksTask = SaveRegionData(worldSaveHelper.GetRegions(WorldSaveHelper.BlocksRegionName),
            new RegionSaveHelper(WorldOptions), worldData.Chunks);

        var entitiesTask = SaveRegionData(worldSaveHelper.GetRegions(WorldSaveHelper.EntitiesRegionName),
            new EntitiesSaveHelper(WorldOptions.RegionSize), worldData.Entities);

        var playersTask = SavePlayers(new PlayerSaveHelper(GameSaveHelper.PlayersFileSystem), worldData.Players);

        var results = await Task.WhenAll(blocksTask, entitiesTask, playersTask);
        bool saved = results.All(saved => saved);
        
        if(saved)
            saveMarker.MarkSaved();

        return saved;
    }

    protected virtual Task<bool> SaveRegionData<T>(WorldRegions worldRegions,
        RegionalChunkedSaveHelper<T> regionalHelper, IReadOnlyDictionary<Vector3Int, T> chunksData) where T : class
    {
        var regionedData = chunksData.GroupBy(c => (1f * c.Key / WorldOptions.RegionSize).Floor());

        foreach(var region in regionedData)
        {
            var regionPosition = region.Key;
            if(worldRegions.HasRegionFile(regionPosition))
            {
                using(var regionReadStream = worldRegions.OpenRegionRead(regionPosition))
                {
                    using var reader = new BinaryReader(regionReadStream);
                    regionalHelper.Read(reader);
                }
            }

            var firstChunkPosition = regionPosition * WorldOptions.RegionSize;
            foreach(var (chunkPosition, chunkData) in region)
            {
                var chunkLocalPosition = chunkPosition - firstChunkPosition;
                regionalHelper.SetChunkData(chunkLocalPosition, chunkData);
            }

            using var regionWriteStream = worldRegions.OpenRegionWrite(regionPosition);
            using var writer = new BinaryWriter(regionWriteStream);
            regionalHelper.Write(writer);
        }
        return Task.FromResult(true);
    }

    protected virtual Task<bool> SavePlayers(PlayerSaveHelper playersSaveHelper,
        IReadOnlyDictionary<ulong, PlayerData> playersData)
    {
        foreach(var (steamId, data) in playersData)
        {
            using var stream = playersSaveHelper.OpenRegionWrite(steamId);
            using var writer = new BinaryWriter(stream);
            data.Write(writer);
        }

        return Task.FromResult(true);
    }
}
