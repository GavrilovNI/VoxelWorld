using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.IO.Helpers;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace VoxelWorld.IO;

public class GameLoader : Component
{
    [Property] protected string GameScene { get; set; } = "scenes/main.scene";
    [Property] protected string ErrorScene { get; set; } = "scenes/mainmenu.scene";

    public virtual async Task<bool> TryLoadGame(BaseFileSystem gameFileSystem)
    {
        var sceneLoaded = Game.ActiveScene.LoadFromFile(GameScene);
        if(!sceneLoaded)
            return false;

        var scene = Game.ActiveScene;
        var game = scene.Components.Get<GameController>(FindMode.EverythingInDescendants);
        if(game is null)
            throw new InvalidOperationException($"Loaded scene doesn't contain {nameof(GameController)}");

        await game.Initialize();

        bool gameLoaded = game.TryLoadGame(gameFileSystem);
        if(!gameLoaded)
        {
            Log.Warning($"Couldn't load game from {gameFileSystem.GetFullPath("/")}");
            Game.ActiveScene.LoadFromFile(ErrorScene);
        }

        return gameLoaded;
    }

    public virtual async Task<(bool loaded, string path)> TryCreateNewGame(BaseFileSystem savesFileSystem, GameInfo gameInfo)
    {
        var path = GetNotTakenSavePath(savesFileSystem, gameInfo.Name);

        var gameFileSystem = savesFileSystem.CreateDirectoryAndSubSystem(path);
        GameSaveHelper helper = new(gameFileSystem);
        helper.SaveGameInfo(in gameInfo);

        bool loaded = await TryLoadGame(gameFileSystem);
        if(!loaded)
        {
            savesFileSystem.DeleteDirectory(path, true);
        }
        return (loaded, path);
    }

    public virtual string GetNotTakenSavePath(BaseFileSystem savesFileSystemm, string fileName)
    {
        string baseName = IOUtils.RemoveInvalidCharacters(fileName);
        baseName = string.IsNullOrWhiteSpace(baseName) ? "New Game" : baseName;
        int addValue = 1;

        string result = baseName;
        while(savesFileSystemm.DirectoryExists(result))
            result = $"{baseName} ({addValue++})";
        return result;
    }
}
