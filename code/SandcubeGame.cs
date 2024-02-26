using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.IO;
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

    [Property] public GameObject WorldPrefab { get; private set; } = null!;
    [Property] public GameObject PlayerPrefab { get; private set; } = null!;
    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;
    [Property] public bool ShouldAnimateBlockTextures { get; private set; } = true;
    public SandcubePlayer LocalPlayer { get; private set; } = null!;

    [Property] protected bool ClickToSpawnPlayer { get; set; } = false;

    private readonly WorldsContainer _worlds = new(); // TODO: make readonly?
    public IReadOnlyWorldsContainer Worlds => _worlds;

    public GameInfo? CurrentGameInfo { get; private set; }
    public GameSaveHelper? CurrentGameSaveHelper { get; private set; }

    public RegistriesContainer Registries { get; } = new();

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

    public bool TryAddWorld(ModedId id, out World world)
    {
        AssertLoadingStatus(LoadingStatus.Loaded);

        if(_worlds.TryGetWorld(id, out world))
            return true;

        if(CurrentGameSaveHelper is null)
        {
            world = null!;
            return false;
        }

        var worldFileSystem = CurrentGameSaveHelper.GetOrCreateWorldFileSystem(id);

        world = CreateWorld(id, worldFileSystem.GetPathFromData("/"));
        _worlds.AddWorld(id, world);
        return true;
    }

    public SandcubePlayer SpawnPlayer(World world, bool enable = true)
    {
        var cloneConfig = new CloneConfig(Transform.World, world.GameObject, false, $"Player");
        var playerGameObject = PlayerPrefab.Clone(cloneConfig);
        playerGameObject.BreakFromPrefab();
        var player = playerGameObject.Components.Get<SandcubePlayer>(true);

        foreach(var worldInitializable in playerGameObject.Components.GetAll<IWorldInitializable>(FindMode.EverythingInSelf))
            worldInitializable.InitializeWorld(world);

        playerGameObject.Enabled = enable;
        return player;
    }

    public async Task LoadMods(IEnumerable<ISandcubeMod> mods)
    {
        AssertInitalizationStatus(InitalizationStatus.NotInitialized, false);

        List<Task> registeringTasks = new();
        foreach(var mod in mods)
        {
            if(_mods.ContainsKey(mod.Id))
                throw new InvalidOperationException($"Mod with id {mod.Id} was already added");
            _mods[mod.Id] = mod;

            var task = mod.RegisterValues(Registries);
            registeringTasks.Add(task);
        }
        await Task.WhenAll(registeringTasks);

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


    private World CreateWorld(string name, string savePath, bool enable = true)
    {
        var cloneConfig = new CloneConfig(Transform.World, GameObject, false, $"World {name}");
        var worldGameObject = WorldPrefab.Clone(cloneConfig);
        worldGameObject.BreakFromPrefab();
        var world = worldGameObject.Components.Get<World>(true);

        foreach(var savePathInitializable in worldGameObject.Components.GetAll<ISavePathInitializable>(FindMode.EverythingInSelf))
            savePathInitializable.InitizlizeSavePath(savePath);

        worldGameObject.Enabled = enable;
        return world;
    }

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

        if(ClickToSpawnPlayer)
        {
            ClickToSpawnPlayer = false;
            if(Worlds.Count > 0)
            {
                var world = Worlds.First().Value;
                LocalPlayer = SpawnPlayer(world);
                foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerInitializable>(FindMode.EverythingInSelfAndDescendants))
                    localPlayerInitializable.InitializeLocalPlayer(LocalPlayer);
            }
        }
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
        if(BlocksTextureMap.UpdateAnimatedTextures())
        {
            foreach(var (_, world) in Worlds)
                world.UpdateTexture(BlocksTextureMap.Texture);
        }
    }

    public void RebuildBlockMeshes(IEnumerable<Block> blocks) =>
        RebuildBlockMeshes(blocks.SelectMany(block => block.BlockStateSet));

    public void RebuildBlockMeshes(IEnumerable<BlockState> blockStates)
    {
        foreach(var blockState in blockStates)
            BlockMeshes.Update(blockState);
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
