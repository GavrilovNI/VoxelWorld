using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using System.IO;
using System.Collections.Generic;
using VoxelWorld.Entities;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Worlds;

namespace VoxelWorld.IO.Helpers;

public class WorldSaveHelper
{
    public static readonly Id BlocksRegionName = new("blocks");
    public static readonly Id EntitiesRegionName = new("entities");

    public readonly BaseFileSystem FileSystem;

    public WorldSaveHelper(BaseFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    public virtual void SaveWorldOptions(in WorldOptions worldOptions)
    {
        var tag = worldOptions.Write();
        using var stream = FileSystem.OpenWrite("world.options");
        using var writer = new BinaryWriter(stream);
        tag.Write(writer);
    }

    public virtual bool TryReadWorldOptions(out WorldOptions worldOptions)
    {
        if(!FileSystem.FileExists("world.options"))
        {
            worldOptions = default;
            return false;
        }

        using var stream = FileSystem.OpenRead("world.options");
        using var reader = new BinaryReader(stream);
        var tag = BinaryTag.Read(reader);
        worldOptions = WorldOptions.Read(tag);
        return true;
    }

    public virtual RegionalSaveHelper GetRegionalHelper(Id id, Vector3IntB regionSize) =>
        new(FileSystem.CreateDirectoryAndSubSystem(id), regionSize, id);

    public virtual void SaveOutOfLimitsEntities(BinaryTag tag)
    {
        using var stream = FileSystem.OpenWrite("out_of_limits.entities");
        using var writer = new BinaryWriter(stream);
        tag.Write(writer);
    }

    public virtual List<Entity> ReadOutOfLimitsEntities(IWorldAccessor world, bool enableEntities = true)
    {
        var subsystem = FileSystem.CreateDirectoryAndSubSystem(EntitiesRegionName);
        if(!subsystem.FileExists("out_of_limits.entities"))
            return new List<Entity>();

        BinaryTag entitiesTag;

        using(var stream = subsystem.OpenRead("out_of_limits.entities"))
        {
            using var reader = new BinaryReader(stream);
            entitiesTag = BinaryTag.Read(reader);
        }

        ListTag listTag = entitiesTag.To<ListTag>();

        List<Entity> result = new();
        foreach(var enityTag in listTag)
        {
            var entiy = Entity.Read(enityTag, world, enableEntities);
            result.Add(entiy);
        }
        return result;
    }
}
