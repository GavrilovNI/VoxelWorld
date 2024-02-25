using Sandbox;
using Sandcube.Data;
using System;
using System.Threading.Tasks;

namespace Sandcube.IO;

public class GameLoader : Component
{
    [Property] protected string GameScene { get; set; } = "scenes/main.scene";
    [Property] protected string ErrorScene { get; set; } = "scenes/mainmenu.scene";

    public virtual async Task<bool> TryLoadGame(BaseFileSystem gameFileSystem)
    {
        var sceneLoaded = GameManager.ActiveScene.LoadFromFile(GameScene);
        if(!sceneLoaded)
            return false;

        var scene = GameManager.ActiveScene;
        var game = scene.Components.Get<SandcubeGame>(FindMode.EverythingInDescendants);
        if(game is null)
            throw new InvalidOperationException($"Loaded scene doesn't contain {nameof(SandcubeGame)}");

        await game.Initialize();

        bool gameLoaded = game.TryLoadGame(gameFileSystem);
        if(!gameLoaded)
        {
            Log.Warning($"Couldn't load game from {gameFileSystem.GetFullPath("/")}");
            GameManager.ActiveScene.LoadFromFile(ErrorScene);
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

    protected string GetNotTakenSavePath(BaseFileSystem savesFileSystemm, string path)
    {
        string result = path;
        int addValue = 1;
        while(savesFileSystemm.DirectoryExists(result))
            result = $"{path} ({addValue++})";
        return result;
    }
}
