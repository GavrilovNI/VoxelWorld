using Sandbox;
using Sandcube.Data;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO.Helpers;

public class WorldSaveHelper
{
    public readonly BaseFileSystem WorldFileSystem;
    public readonly BaseFileSystem RegionsFileSystem;

    public WorldSaveHelper(BaseFileSystem worldFileSystem)
    {
        WorldFileSystem = worldFileSystem;
        RegionsFileSystem = worldFileSystem.CreateDirectoryAndSubSystem("regions");
    }

    public virtual void SaveWorldOptions(in WorldOptions worldOptions)
    {
        using var stream = WorldFileSystem.OpenWrite("world.options");
        using var writer = new BinaryWriter(stream);
        worldOptions.Write(writer);
    }

    public virtual bool TryReadWorldOptions(out WorldOptions worldOptions)
    {
        if(!WorldFileSystem.FileExists("world.options"))
        {
            worldOptions = default;
            return false;
        }

        using var stream = WorldFileSystem.OpenRead("world.options");
        using var reader = new BinaryReader(stream);
        worldOptions = WorldOptions.Read(reader);
        return true;
    }

    public virtual string GetRegionFile(in Vector3Int regionPosition) =>
        $"{regionPosition.x}.{regionPosition.y}.{regionPosition.z}.region";

    public virtual bool HasRegionFile(in Vector3Int regionPosition) =>
        RegionsFileSystem.FileExists(GetRegionFile(regionPosition));

    public virtual Dictionary<Vector3Int, string> GetAllRegionFiles()
    {
        var posibleFiles = RegionsFileSystem.FindFile("/", "*.*.*.region", false);

        Dictionary<Vector3Int, string> result = new();
        foreach(var posibleFile in posibleFiles)
        {
            var parts = posibleFile.Split('.', 4);
            if(parts.Length != 4)
                continue;

            if(!int.TryParse(parts[0], out int x) ||
                !int.TryParse(parts[1], out int y) ||
                !int.TryParse(parts[2], out int z))
                continue;

            result.Add(new(x, y, z), posibleFile);
        }
        return result;
    }

    public virtual Stream OpenRegionRead(in Vector3Int regionPosition, FileMode fileMode = FileMode.Open)
    {
        string fileName = GetRegionFile(regionPosition);
        return RegionsFileSystem.OpenRead(fileName, fileMode);
    }

    public virtual Stream OpenRegionWrite(in Vector3Int regionPosition, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetRegionFile(regionPosition);
        return RegionsFileSystem.OpenWrite(fileName, fileMode);
    }
}
