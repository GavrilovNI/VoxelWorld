using Sandbox;
using Sandcube.Data;
using Sandcube.Registries;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO.Helpers;

public class GameSaveHelper
{
    public readonly BaseFileSystem GameFileSystem;
    public readonly BaseFileSystem WorldsFileSystem;
    public readonly BaseFileSystem PlayersFileSystem;

    public GameSaveHelper(BaseFileSystem gameFileSystem)
    {
        GameFileSystem = gameFileSystem;
        WorldsFileSystem = gameFileSystem.CreateDirectoryAndSubSystem("worlds");
        PlayersFileSystem = gameFileSystem.CreateDirectoryAndSubSystem("players");
    }

    public static Dictionary<string, GameInfo> GetAllSaves(BaseFileSystem savesFileSystem)
    {
        var possibleSaves = savesFileSystem.FindDirectory("/", "*", false);

        Dictionary<string, GameInfo> result = new();

        foreach(var possibleSave in possibleSaves)
        {
            var helper = new GameSaveHelper(savesFileSystem.CreateSubSystem(possibleSave));
            if(helper.TryReadGameInfo(out var gameInfo))
                result.Add(possibleSave, gameInfo);
        }
        return result;
    }

    public virtual void SaveGameInfo(in GameInfo gameInfo)
    {
        using var stream = GameFileSystem.OpenWrite("game.info");
        using var writer = new BinaryWriter(stream);
        writer.Write(gameInfo);
    }

    public virtual bool TryReadGameInfo(out GameInfo gameInfo)
    {
        if(!GameFileSystem.FileExists("game.info"))
        {
            gameInfo = default;
            return false;
        }

        using var stream = GameFileSystem.OpenRead("game.info");
        using var reader = new BinaryReader(stream);
        gameInfo = GameInfo.Read(reader);
        return true;
    }

    public static bool TryConvertWorldDirectoryToId(string worldDirectory, out ModedId id) =>
        ModedId.TryParse(worldDirectory.Replace('-', ':'), out id);
    public static string ConvertWorldIdToDirectory(ModedId id) => $"{id.ModId}-{id.Name}";

    public virtual bool HasWorld(ModedId id) => WorldsFileSystem.DirectoryExists(ConvertWorldIdToDirectory(id));
    public virtual bool TryGetWorldFileSystem(ModedId id, out BaseFileSystem worldFileSystem)
    {
        var worldDirectory = ConvertWorldIdToDirectory(id);
        if(!WorldsFileSystem.DirectoryExists(worldDirectory))
        {
            worldFileSystem = null!;
            return false;
        }

        worldFileSystem = WorldsFileSystem.CreateSubSystem(worldDirectory);
        return true;
    }

    public virtual BaseFileSystem GetOrCreateWorldFileSystem(ModedId id)
    {
        var worldDirectory = ConvertWorldIdToDirectory(id);
        return WorldsFileSystem.CreateDirectoryAndSubSystem(worldDirectory);
    }

    public virtual Dictionary<ModedId, BaseFileSystem> GetAllWorldFileSystems()
    {
        var posibleDirectories = WorldsFileSystem.FindDirectory("/", "*", false);

        Dictionary<ModedId, BaseFileSystem> result = new();
        foreach(var posibleDirectory in posibleDirectories)
        {
            if(!TryConvertWorldDirectoryToId(posibleDirectory, out var id))
                continue;

            result.Add(id, WorldsFileSystem.CreateSubSystem(posibleDirectory));
        }
        return result;
    }
}
