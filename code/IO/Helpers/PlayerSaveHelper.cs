using Sandbox;
using System.Collections.Generic;
using System.IO;

namespace VoxelWorld.IO.Helpers;

public class PlayerSaveHelper
{
    public readonly BaseFileSystem FileSystem;

    public PlayerSaveHelper(BaseFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    public virtual string GetPlayerFile(ulong steamId) => $"{steamId}.player";

    public virtual bool HasPlayerFile(ulong steamId) =>
        FileSystem.FileExists(GetPlayerFile(steamId));

    public virtual Dictionary<ulong, string> GetAllPlayerFiles()
    {
        var posibleFiles = FileSystem.FindFile("/", $"*.player", false);

        Dictionary<ulong, string> result = new();
        foreach(var posibleFile in posibleFiles)
        {
            var parts = posibleFile.Split('.', 2);
            if(parts.Length != 2)
                continue;

            if(!ulong.TryParse(parts[0], out ulong steamId))
                continue;

            result.Add(steamId, posibleFile);
        }
        return result;
    }

    public virtual Stream OpenPlayerRead(ulong steamId, FileMode fileMode = FileMode.Open)
    {
        string fileName = GetPlayerFile(steamId);
        return FileSystem.OpenRead(fileName, fileMode);
    }

    public virtual Stream OpenPlayerWrite(ulong steamId, FileMode fileMode = FileMode.Create)
    {
        string fileName = GetPlayerFile(steamId);
        return FileSystem.OpenWrite(fileName, fileMode);
    }
}
