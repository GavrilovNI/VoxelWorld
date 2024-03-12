using Sandbox;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.IO.Helpers;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.Mth;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class ChunkEntitiesLoader : ChunkCreationStage, IWorldInitializationListener
{
    [Property, Category("Debug")] public bool BreakFromPrefab { get; set; } = true;

    protected World World { get; private set; } = null!;
    protected BaseFileSystem WorldFileSystem { get; private set; } = null!;
    protected WorldOptions WorldOptions { get; private set; }

    public void OnWorldInitialized(World world)
    {
        World = world;
        WorldFileSystem = world.WorldFileSystem!;
        WorldOptions = world.WorldOptions;
    }

    public override Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!TryLoadEntities(chunk.Position, out var entities))
            return Task.FromResult(false);

        cancellationToken.ThrowIfCancellationRequested();
        foreach(var entity in entities)
        {
            if(entity is Player)
            {
                entity.Destroy();
                continue;
            }
            chunk.AddEntity(entity);
        }
        return Task.FromResult(true);
    }

    public virtual bool TryLoadEntities(Vector3Int chunkPosition, out List<Entity> entities, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        WorldSaveHelper worldSaveHelper = new(WorldFileSystem);
        RegionalSaveHelper saveHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.EntitiesRegionName, WorldOptions.RegionSize);

        if(!saveHelper.TryLoadOneChunkOnly(chunkPosition, out var entitiesTags))
        {
            entities = null!;
            return false;
        }

        entities = LoadEntitiesFromTag(entitiesTags, World, enableEntities);
        return true;
    }

    protected virtual List<Entity> LoadEntitiesFromTag(BinaryTag tag, IWorldAccessor world, bool enableEntities = true)
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
