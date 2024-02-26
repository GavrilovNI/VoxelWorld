using Sandbox;
using Sandcube.Blocks;
using Sandcube.Data;
using Sandcube.IO;
using Sandcube.Items;
using Sandcube.Meshing.Blocks;
using Sandcube.Mods;
using Sandcube.Players;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube;

public sealed class SandcubeGame : Component
{
    public static event Action? Initialized;
    public static InitalizationStatus InitalizationStatus { get; private set; } = InitalizationStatus.NotInitialized;
    public static LoadingStatus LoadingStatus { get; private set; } = LoadingStatus.NotLoaded;

    
    private static SandcubeGame? _instance = null;
    public static SandcubeGame? Instance
    {
        get
        {
            if(_instance.IsValid())
                return _instance;
            return null;
        }
        private set => _instance = value;
    }

    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;
    [Property] public bool ShouldAnimateBlockTextures { get; private set; } = true;


    public GameInfo? CurrentGameInfo { get; private set; }
    public GameSaveHelper? CurrentGameSaveHelper { get; private set; }

    public Registry<Block> BlocksRegistry { get; } = new();
    public Registry<Item> ItemsRegistry { get; } = new();
    public PathedTextureMap BlocksTextureMap { get; } = new("textures/", 3, new Color32(255, 0, 255), 4);
    public BlockMeshMap BlockMeshes { get; } = new();

    public SandcubeBaseMod BaseMod { get; private set; } = null!;

    private readonly Dictionary<Id, ISandcubeMod> _mods = new();

    public async Task Initialize()
    {
        AssertInitalizationStatus(InitalizationStatus.NotInitialized);

        InitalizationStatus = InitalizationStatus.Initializing;
        Instance = this;

        await LoadAllMods();

        InitalizationStatus = InitalizationStatus.Initialized;
        Initialized?.Invoke();
    }

    public bool TryLoadGame(BaseFileSystem baseFileSystem)
    {
        AssertInitalizationStatus(InitalizationStatus.Initialized);
        AssertLoadingStatus(LoadingStatus.NotLoaded);

        LoadingStatus = LoadingStatus.Loading;

        GameSaveHelper helper = new(baseFileSystem);
        if(!helper.TryReadGameInfo(out var gameInfo))
        {
            LoadingStatus = LoadingStatus.NotLoaded;
            return false;
        }

        CurrentGameInfo = gameInfo;
        CurrentGameSaveHelper = helper;

        LoadingStatus = LoadingStatus.Loaded;

        foreach(var (_, mod) in _mods)
            mod.OnGameLoaded();

        return true;
    }

    public async Task LoadMods(IEnumerable<ISandcubeMod> mods)
    {
        AssertInitalizationStatus(InitalizationStatus.NotInitialized, false);

        List<Task> blocksRegisteringTasks = new();
        foreach(var mod in mods)
        {
            if(_mods.ContainsKey(mod.Id))
                throw new InvalidOperationException($"Mod with id {mod.Id} was already added");
            _mods[mod.Id] = mod;

            blocksRegisteringTasks.Add(mod.RegisterBlocks(BlocksRegistry));
        }
        await Task.WhenAll(blocksRegisteringTasks);

        RebuildBlockMeshes();

        List<Task> itemsRegisteringTasks = new();
        foreach(var mod in mods)
            itemsRegisteringTasks.Add(mod.RegisterItems(ItemsRegistry));
        await Task.WhenAll(itemsRegisteringTasks);

        foreach(var mod in mods)
            mod.OnLoaded();

        if(LoadingStatus == LoadingStatus.Loaded)
        {
            foreach(var mod in mods)
                mod.OnGameLoaded();
        }
    }

    public ISandcubeMod? GetMod(Id id) => _mods!.GetValueOrDefault(id, null);
    public bool IsModLoaded(Id id) => _mods.ContainsKey(id);


    protected override void OnEnabled()
    {
        if(Instance.IsValid() && Instance != this)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(SandcubeGame)}. Destroying {this}...");
            Destroy();
            return;
        }
    }

    protected override void OnStart()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        if(InitalizationStatus == InitalizationStatus.NotInitialized)
            _ = Initialize();
    }

    protected override void OnUpdate()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        if(InitalizationStatus != InitalizationStatus.Initialized)
            return;

        if(ShouldAnimateBlockTextures)
            AnimateBlockTextures();
    }

    protected override void OnDisabled()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        if(IsValid)
        {
            Log.Warning($"{nameof(SandcubeGame)} was disabled. It's fine if it being destroyed or scene is unloading. Destroying {this} ...");
            Destroy();
        }
    }

    protected override void OnDestroy()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        InitalizationStatus = InitalizationStatus.NotInitialized;
        LoadingStatus = LoadingStatus.NotLoaded;
        Instance = null;
    }

    private void AnimateBlockTextures()
    {
        //TODO:
    }

    private void RebuildBlockMeshes()
    {
        BlockMeshes.Clear();
        foreach(var block in BlocksRegistry.All)
        {
            foreach(var blockState in block.Value.BlockStateSet)
                BlockMeshes.Add(blockState);
        }
    }

    private async Task LoadAllMods()
    {
        BaseMod = new();
        List<ISandcubeMod> modsToLoad = new() { BaseMod };
        await LoadMods(modsToLoad);
    }

    
    private void AssertValid()
    {
        if(!IsValid)
            throw new InvalidOperationException($"{nameof(SandcubeGame)} {this} is not valid");
        if(Instance.IsValid() && Instance != this)
            throw new InvalidOperationException($"{nameof(SandcubeGame)} {this} is duplicated instance");
        if(Scene.IsEditor)
            throw new InvalidOperationException($"{nameof(SandcubeGame)} {this} can't run in editor");
    }

    private void AssertInitalizationStatus(InitalizationStatus initalizationStatus, bool equal = true)
    {
        AssertValid();
        bool statusEqual = InitalizationStatus == initalizationStatus;
        if(statusEqual != equal)
            throw new InvalidOperationException($"{nameof(SandcubeGame)}'s {nameof(InitalizationStatus)} is{(equal ? string.Empty : " not")} {initalizationStatus}");
    }

    private void AssertLoadingStatus(LoadingStatus loadingStatus, bool equal = true)
    {
        AssertValid();
        bool statusEqual = LoadingStatus == loadingStatus;
        if(statusEqual != equal)
            throw new InvalidOperationException($"{nameof(SandcubeGame)}'s {nameof(LoadingStatus)} is{(equal ? string.Empty : " not")} {loadingStatus}");
    }
}
