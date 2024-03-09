using Sandbox;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.IO.Helpers;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Worlds.Loading;

public class EntitiesCreator : Component
{
    [Property] protected World World { get; set; } = null!;
    [Property] protected GameObject EntitiesParent { get; set; } = null!;

    protected BaseFileSystem WorldFileSystem => World.WorldFileSystem!;
    protected WorldOptions WorldOptions => World.WorldOptions;

    [Property, Category("Debug")] public bool BreakFromPrefab { get; set; } = true;

    public List<Entity> LoadOrCreateEntitiesForChunk2(Vector3Int chunkPosition, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        WorldSaveHelper worldSaveHelper = new(WorldFileSystem);
        RegionalSaveHelper saveHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.EntitiesRegionName, WorldOptions.RegionSize);
        
        if(!saveHelper.TryLoadOneChunkOnly(chunkPosition, out var entitiesTags))
            return new List<Entity>();

        return CreateEntities(entitiesTags, World, enableEntities);
    }

    protected virtual List<Entity> CreateEntities(BinaryTag tag, IWorldAccessor world, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        ListTag listTag = tag.To<ListTag>();

        List<Entity> result = new();
        foreach(var enityTag in listTag)
        {
            var entiy = Entity.Read(enityTag, world, enableEntities);
            result.Add(entiy);
        }
        return result;
    }
}
