using Sandbox;
using Sandcube.Data;
using System.IO;

namespace Sandcube.IO.Helpers;

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
        using var stream = FileSystem.OpenWrite("world.options");
        using var writer = new BinaryWriter(stream);
        worldOptions.Write(writer);
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
        worldOptions = WorldOptions.Read(reader);
        return true;
    }

    public virtual WorldRegions GetRegions(Id id) => new(this, id);
}
