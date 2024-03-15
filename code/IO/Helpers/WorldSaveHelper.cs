using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using System.IO;

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

    public virtual RegionalSaveHelper GetRegionalHelper(Id id, Vector3Int regionSize) =>
        new(FileSystem.CreateDirectoryAndSubSystem(id), regionSize, id);
}
