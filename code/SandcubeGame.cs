using Sandbox;
using Sandcube.Base;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Exceptions;
using Sandcube.IO;
using Sandcube.IO.Helpers;
using Sandcube.Meshing.Blocks;
using Sandcube.Mods;
using Sandcube.Mods.Base;
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
    public static event Action<World>? WorldAdded;

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

    [Property] public GameObject ModsParent { get; private set; } = null!;
    [Property] public GameObject WorldsParent { get; private set; } = null!;
    [Property] public GameObject BaseModPrefab { get; private set; } = null!;
    [Property] public GameObject WorldPrefab { get; private set; } = null!;
    [Property] public WorldOptions DefaultWorldOptions { get; private set; } = new() { ChunkSize = 16, RegionSize = 4 };
    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;
    [Property] public PlayerSpawner PlayerSpawner { get; private set; } = null!;

    [Property] public bool ShouldAnimateBlockTextures { get; private set; } = true;
    [Property] public Material OpaqueVoxelsMaterial { get; private set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; private set; } = null!;


    private readonly WorldsContainer _worlds = new(); // TODO: make readonly?
    public IReadOnlyWorldsContainer Worlds => _worlds;

    public GameInfo? CurrentGameInfo { get; private set; }
    public GameSaveHelper? CurrentGameSaveHelper { get; private set; }

    public RegistriesContainer Registries { get; } = new();

    public PathedTextureMap BlocksTextureMap { get; } = new("textures/", 3, new Color32(255, 0, 255), 4);
    public BlockMeshMap BlockMeshes { get; } = new();

    public SandcubeBaseMod BaseMod { get; private set; } = null!;

    private readonly Dictionary<Id, ISandcubeMod> _mods = new();

    private Task<bool>? _savingTask = null;
    private bool _wasClosingGame = false;


    public async Task Initialize()
    {
        AssertInitalizationStatus(InitalizationStatus.NotInitialized);

        InitalizationStatus = InitalizationStatus.Initializing;
        Instance = this;

        var loaded = TryCloneModFrom<SandcubeBaseMod>(BaseModPrefab, out var baseMod);
        if(!loaded)
            throw new InvalidOperationException($"couldn't create {nameof(SandcubeBaseMod)}");

        BaseMod = baseMod;
        await LoadMods(new ISandcubeMod[] { BaseMod });

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

    public async Task<bool> SaveGame()
    {
        ThreadSafe.AssertIsMainThread();
        AssertInitalizationStatus(InitalizationStatus.Initialized);
        AssertLoadingStatus(LoadingStatus.Loaded);

        if(_savingTask is not null)
            return await _savingTask;

        var savers = Scene.Components.GetAll<ISaver>(FindMode.EnabledInSelfAndDescendants);

        List<Task<bool>> tasks = new();
        foreach(var saver in savers)
            tasks.Add(saver.Save());

        TaskCompletionSource<bool> taskCompletionSource = new();
        _savingTask = taskCompletionSource.Task;
        var results = await Task.WhenAll(tasks);
        var result = results.All(saved => saved);
        taskCompletionSource.SetResult(result);
        _savingTask = null;
        return result;
    }

    public bool TryAddWorld(ModedId id, out World world)
    {
        AssertLoadingStatus(LoadingStatus.Loaded);

        if(_worlds.TryGetWorld(id, out world))
            return false;

        if(CurrentGameSaveHelper is null)
        {
            world = null!;
            return false;
        }

        var worldFileSystem = CurrentGameSaveHelper.GetOrCreateWorldFileSystem(id);

        world = CreateWorld(id, worldFileSystem);
        _worlds.AddWorld(world);
        WorldAdded?.Invoke(world);
        return true;
    }

    public async Task LoadMods(IEnumerable<ISandcubeMod> mods)
    {
        AssertInitalizationStatus(InitalizationStatus.NotInitialized, false);

        Dictionary<Id, RegistriesContainer> modRegistries = new();
        
        List<Task> registeringTasks = new();
        foreach(var mod in mods)
        {
            if(_mods.ContainsKey(mod.Id))
                throw new InvalidOperationException($"Mod with id {mod.Id} was already added");

            if(mod is Component modComponent)
                modComponent.GameObject.Parent = ModsParent;

            RegistriesContainer registries = new();
            var task = mod.RegisterValues(registries);
            registeringTasks.Add(task);
            modRegistries.Add(mod.Id, registries);
        }
        await Task.WhenAll(registeringTasks);

        var regsiteredMods = mods.ToDictionary(m => m.Id, m => m);
        foreach(var (modId, registries) in modRegistries)
        {
            var usedIdsFromMod = 
                registries.All(registry => registry.Value.All.All(registerable => registerable.Key.ModId == modId));

            if(!usedIdsFromMod)
            {
                Log.Warning($"Mod {modId} tried to register values with different mod id, skipping mod");
                regsiteredMods.Remove(modId);
                continue;
            }
            Registries.Add(registries);
        }

        foreach(var (id, mod) in regsiteredMods)
        {
            _mods[id] = mod;
            mod.OnLoaded();
        }

        if(LoadingStatus == LoadingStatus.Loaded)
        {
            foreach(var mod in mods)
                mod.OnGameLoaded();
        }
    }

    public ISandcubeMod? GetMod(Id id) => _mods!.GetValueOrDefault(id, null);
    public bool IsModLoaded(Id id) => _mods.ContainsKey(id);


    private World CreateWorld(ModedId id, BaseFileSystem fileSystem, bool enable = true)
    {
        var cloneConfig = new CloneConfig(Transform.World, WorldsParent, false, $"World {id}");
        var worldGameObject = WorldPrefab.Clone(cloneConfig);
        worldGameObject.BreakFromPrefab();
        var world = worldGameObject.Components.Get<World>(true);

        var worldOptions = DefaultWorldOptions with { Seed = CurrentGameInfo!.Value.Seed + id.GetHashCode() };
        world.Initialize(id, fileSystem, worldOptions);

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

        ModsParent ??= GameObject;
        WorldsParent ??= GameObject;

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

        if(!_wasClosingGame && Game.IsClosing && _savingTask is null)
            _ = SaveGame();
        _wasClosingGame = Game.IsClosing;
    }

    protected override void OnDisabled()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        foreach(var (_, mod) in _mods)
            mod.OnUnloaded();

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
            OpaqueVoxelsMaterial.Attributes.Set("color", BlocksTextureMap.Texture);
    }

    public void RebuildBlockMeshes(IEnumerable<Block> blocks) =>
        RebuildBlockMeshes(blocks.SelectMany(block => block.BlockStateSet));

    public void RebuildBlockMeshes(IEnumerable<BlockState> blockStates)
    {
        foreach(var blockState in blockStates)
            BlockMeshes.Update(blockState);
    }

    public static bool TryCreateMod(TypeDescription type, out ISandcubeMod mod)
    {
        var targetType = type.TargetType;
        if(!targetType.IsAssignableTo(typeof(ISandcubeMod)))
        {
            mod = null!;
            return false;
        }

        if(targetType.IsAssignableTo(typeof(Component)))
        {
            GameObject modObject = new();
            mod = (modObject.Components.Create(type) as ISandcubeMod)!;
            modObject.Name = mod.Id;
            return true;
        }

        try
        {
            mod = type.Create<ISandcubeMod>();
            return true;
        }
        catch(MissingMethodException)
        {
            mod = default!;
            return false;
        }
    }

    public static bool TryCreateMod<T>(out T mod) where T : ISandcubeMod
    {
        var created = TryCreateMod(TypeLibrary.GetType<T>(), out var createdMod);
        mod = created ? (T)createdMod : default!;
        return created;
    }

    public static bool TryCloneModFrom(GameObject modObject, out ISandcubeMod mod) =>
        TryCloneModFrom<ISandcubeMod>(modObject, out mod);

    public static bool TryCloneModFrom<T>(GameObject modObject, out T mod) where T : ISandcubeMod
    {
        ArgumentNotValidException.ThrowIfNotValid(modObject);
        modObject = modObject.Clone();
        modObject.BreakFromPrefab();
        mod = modObject.Components.Get<T>(FindMode.EverythingInSelfAndDescendants);

        bool found = mod is not null;
        if(found)
            modObject.Name = mod!.Id;
        else
            modObject.Destroy();

        return found;
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
