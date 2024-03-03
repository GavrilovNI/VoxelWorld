using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandcube.IO.Helpers;

public class WorldRegions
{
    public readonly Id Id;
    public readonly WorldSaveHelper WorldSaveHelper;
    public readonly BaseFileSystem FileSystem;

    public WorldRegions(WorldSaveHelper saveHelper, Id id)
    {
        WorldSaveHelper = saveHelper;
        Id = id;
        FileSystem = WorldSaveHelper.FileSystem.CreateDirectoryAndSubSystem(Id);
    }


    public virtual string GetRegionFile(in Vector3Int regionPosition) =>
        $"{regionPosition.x}.{regionPosition.y}.{regionPosition.z}.{Id}";

    public virtual bool HasRegionFile(in Vector3Int regionPosition) =>
        FileSystem.FileExists(GetRegionFile(regionPosition));

    public virtual Dictionary<Vector3Int, string> GetAllRegionFiles()
    {
        var posibleFiles = FileSystem.FindFile("/", $"*.*.*.{Id}", false);

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
        return FileSystem.OpenRead(fileName, fileMode);
    }

    public virtual Stream OpenRegionWrite(in Vector3Int regionPosition, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetRegionFile(regionPosition);
        return FileSystem.OpenWrite(fileName, fileMode);
    }
}
