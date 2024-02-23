using Sandbox;
using Sandcube.Mth;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO.World;

public class WorldSaveHelper
{
    public readonly BaseFileSystem WorldFileSystem;
    public readonly BaseFileSystem RegionsFileSystem;

    public WorldSaveHelper(BaseFileSystem worldFileSystem)
    {
        WorldFileSystem = worldFileSystem;
        RegionsFileSystem = worldFileSystem.CreateDirectoryAndSubSystem("regions");
    }

    public virtual void SaveWorldOptions(in WorldSaveOptions options)
    {
        using var stream = WorldFileSystem.OpenWrite("world.options");
        using var writer = new BinaryWriter(stream);
        options.Write(writer);
    }

    public virtual bool TryReadWorldOptions(out WorldSaveOptions options)
    {
        if(!WorldFileSystem.FileExists("world.options"))
        {
            options = default;
            return false;
        }

        using var stream = WorldFileSystem.OpenRead("world.options");
        using var reader = new BinaryReader(stream);
        options = WorldSaveOptions.Read(reader);
        return true;
    }

    public virtual string GetRegionFileName(in Vector3Int regionPosition) =>
        $"{regionPosition.x}.{regionPosition.y}.{regionPosition.z}.region";

    public virtual bool HasRegionFile(in Vector3Int regionPosition) =>
        RegionsFileSystem.FileExists(GetRegionFileName(regionPosition));

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
        string fileName = GetRegionFileName(regionPosition);
        return RegionsFileSystem.OpenRead(fileName, fileMode);
    }

    public virtual Stream OpenRegionWrite(in Vector3Int regionPosition, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetRegionFileName(regionPosition);
        return RegionsFileSystem.OpenWrite(fileName, fileMode);
    }
}
