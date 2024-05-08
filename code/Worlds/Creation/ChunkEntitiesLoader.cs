using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.Entities;
using VoxelWorld.IO.Helpers;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds.Creation;

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
        WorldOptions = world.Options;
    }

    public override async Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.MainThread();
        var entities = await TryLoadEntities(chunk.Position);
        if(entities is null)
            return false;

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
        return true;
    }

    public virtual async Task<List<Entity>?> TryLoadEntities(Vector3IntB chunkPosition, bool enableEntities = true)
    {
        ThreadSafe.AssertIsMainThread();

        WorldSaveHelper worldSaveHelper = new(WorldFileSystem);
        RegionalSaveHelper saveHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.EntitiesRegionName, WorldOptions.RegionSize);

        var entitiesTags = await Task.RunInThreadAsync(() =>
        {
            bool loaded = saveHelper.TryLoadOneChunkOnly(chunkPosition, out var entitiesTags);
            return loaded ? entitiesTags : null;
        });

        if(entitiesTags is null)
            return null;

        return CreateEntitiesFromTag(entitiesTags, World, enableEntities);
    }

    protected virtual List<Entity> CreateEntitiesFromTag(BinaryTag tag, IWorldAccessor world, bool enableEntities = true)
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
