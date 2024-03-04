﻿using Sandbox;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.IO.Helpers;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Worlds.Loading;

public class EntitiesCreator : Component
{
    [Property] protected World World { get; set; } = null!;
    [Property] protected GameObject EntitiesParent { get; set; } = null!;

    protected BaseFileSystem? WorldFileSystem => World.WorldFileSystem;
    protected WorldOptions WorldOptions => World.WorldOptions;

    [Property, Category("Debug")] public bool BreakFromPrefab { get; set; } = true;

    public List<Entity> LoadOrCreateEntitiesForChunk(Vector3Int chunkPosition, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        var entitiesData = LoadEntitiesDataForChunk(chunkPosition, enableEntities);
        if(entitiesData is not null)
            return CreateEntities(entitiesData, World, enableEntities);

        return new List<Entity>();
    }

    protected virtual EntitiesData? LoadEntitiesDataForChunk(Vector3Int chunkPosition, bool enableEntities = true)
    {
        var regionPosition = (1f * chunkPosition / WorldOptions.RegionSize).Floor();
        var localChunkPosition = chunkPosition - regionPosition * WorldOptions.RegionSize;

        var worldSaveHelper = new WorldSaveHelper(WorldFileSystem!);
        var entitiesSaveHelper = new EntitiesSaveHelper(WorldOptions.RegionSize);
        var entitiesRegions = worldSaveHelper.GetRegions(WorldSaveHelper.EntitiesRegionName);

        if(entitiesRegions.HasRegionFile(regionPosition))
        {
            using(var regionReadStream = entitiesRegions.OpenRegionRead(regionPosition))
            {
                using var reader = new BinaryReader(regionReadStream);
                if(entitiesSaveHelper.ReadOnlyOneChunk(reader, localChunkPosition))
                {
                    return entitiesSaveHelper.GetChunkData(localChunkPosition)!;
                }
            }
        }

        return null;
    }

    protected virtual List<Entity> CreateEntities(EntitiesData data, IWorldAccessor? world, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        List<Entity> result = new();
        foreach(var entityData in data)
        {
            using var stream = new MemoryStream(entityData);
            using var reader = new BinaryReader(stream);

            var entity = Entity.Read(reader, world, enableEntities);
            result.Add(entity);
        }
        return result;
    }
}