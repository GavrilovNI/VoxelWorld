using Sandbox;
using Sandbox.Utility;
using VoxelWorld.Players;
using VoxelWorld.Blocks;
using VoxelWorld.Blocks.States;
using VoxelWorld.Data;
using VoxelWorld.Entities;
using VoxelWorld.Exceptions;
using VoxelWorld.IO;
using VoxelWorld.IO.Helpers;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mods;
using VoxelWorld.Mods.Base;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoxelWorld.Crafting.Recipes;
using VoxelWorld.Mth;

namespace VoxelWorld;

public sealed class GameController : Component
{
    public static event Action? Initialized;
    public static event Action<World>? WorldAdded;

    public static InitializationStatus InitializationStatus { get; private set; } = InitializationStatus.NotInitialized;
    public static LoadingStatus LoadingStatus { get; private set; } = LoadingStatus.NotLoaded;

    
    private static GameController? _instance = null;
    public static GameController? Instance
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
    public WorldOptions DefaultWorldOptions { get; private set; } = new();
    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;
    [Property] public PlayerSpawner PlayerSpawner { get; private set; } = null!;

    [Property] public bool ShouldAnimateBlockTextures { get; private set; } = true;
    [Property] public Material OpaqueVoxelsMaterial { get; private set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; private set; } = null!;
    [Property] public Material OpaqueItemsMaterial { get; private set; } = null!;
    [Property] public Material TranslucentItemsMaterial { get; private set; } = null!;

    public Player? LocalPlayer
    {
        get
        {
            if(TryGetPlayer(Steam.SteamId, out var player))
                return player;
            return null;
        }
    }


    private readonly Dictionary<ulong, Player?> _players = new();


    private readonly WorldsContainer _worlds = new(); // TODO: make readonly?
    public IReadOnlyWorldsContainer Worlds => _worlds;

    public GameInfo? CurrentGameInfo { get; private set; }
    public GameSaveHelper? CurrentGameSaveHelper { get; private set; }

    public RegistriesContainer Registries { get; } = new();
    public RecipesContainer Recipes { get; } = new();

    public PathedTextureMap BlocksTextureMap { get; } = new("textures/", 3, new Color32(255, 0, 255), 4);
    public BlockMeshMap BlockMeshes { get; } = new();

    public PathedTextureMap ItemsTextureMap { get; } = new("textures/", 3, new Color32(255, 0, 255), 4);

    private readonly Dictionary<Id, IMod> _mods = new();

    private Task<bool>? _savingTask = null;
    private bool _wasClosingGame = false;


    public async Task Initialize()
    {
        AssertInitializationStatus(InitializationStatus.NotInitialized);

        InitializationStatus = InitializationStatus.Initializing;
        Instance = this;

        var loaded = TryCloneModFrom<BaseMod>(BaseModPrefab, out var baseMod);
        if(!loaded)
            throw new InvalidOperationException($"couldn't create {nameof(BaseMod)}");

        await LoadMods(new IMod[] { baseMod });

        InitializationStatus = InitializationStatus.Initialized;
        Initialized?.Invoke();
    }

    public bool TryLoadGame(BaseFileSystem baseFileSystem)
    {
        AssertInitializationStatus(InitializationStatus.Initialized);
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
        AssertInitializationStatus(InitializationStatus.Initialized);
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

        if(!result)
            Log.Warning("Couldn't save game");

        taskCompletionSource.SetResult(result);
        _savingTask = null;
        return result;
    }

    public async Task<World?> TryAddWorld(ModedId id)
    {
        AssertLoadingStatus(LoadingStatus.Loaded);

        if(_worlds.TryGetWorld(id, out var world))
            return null;

        if(CurrentGameSaveHelper is null)
            return null;

        var worldFileSystem = CurrentGameSaveHelper.GetOrCreateWorldFileSystem(id);

        world = await CreateWorld(id, worldFileSystem);
        _worlds.AddWorld(world);
        WorldAdded?.Invoke(world);

        if(!LocalPlayer.IsValid() && world.Id == BaseMod.Instance!.MainWorldId)
            _ = TryRespawnPlayer(Steam.SteamId);

        return world;
    }

    public async Task LoadMods(IEnumerable<IMod> mods)
    {
        AssertInitializationStatus(InitializationStatus.NotInitialized, false);

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

        foreach(var (_, mod) in regsiteredMods)
            mod.RegisterRecipes(Recipes);

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

    public IMod? GetMod(Id id) => _mods!.GetValueOrDefault(id, null);
    public bool IsModLoaded(Id id) => _mods.ContainsKey(id);


    private async Task<World> CreateWorld(ModedId id, BaseFileSystem fileSystem, bool enable = true)
    {
        var cloneConfig = new CloneConfig(Transform.World, WorldsParent, false, $"World {id}");
        var worldGameObject = WorldPrefab.Clone(cloneConfig);
        worldGameObject.BreakFromPrefab();
        var world = worldGameObject.Components.Get<World>(true);

        var worldOptions = DefaultWorldOptions with { Seed = CurrentGameInfo!.Value.Seed + id.GetHashCode() };
        await world.Initialize(id, fileSystem, worldOptions);
        return world;
    }

    public bool HasPlayer(ulong steamId) => TryGetPlayer(steamId, out _);

    public bool TryGetPlayer(ulong steamId, out Player player)
    {
        lock(_players)
        {
            if(_players.TryGetValue(steamId, out player!) && player.IsValid())
                return true;
        }

        player = null!;
        return false;
    }

    public bool WasPlayerSpawned(ulong steamId)
    {
        lock(_players)
        {
            return _players.ContainsKey(steamId);
        }
    }

    public async Task<Player?> TryRespawnPlayer(ulong steamId)
    {
        if(TryGetPlayer(steamId, out var player))
            player.Destroy();

        if(_worlds.TryGetWorld(BaseMod.Instance!.MainWorldId, out var world))
        {
            EntitySpawnConfig spawnConfig = new(world, true);
            player = await PlayerSpawner.SpawnPlayer(steamId, spawnConfig, CancellationToken.None);

            if(player.IsValid())
            {
                lock(_players)
                {
                    if(HasPlayer(steamId))
                    {
                        player.Destroy();
                        return null;
                    }

                    _players[player.SteamId] = player;

                    player!.Destroyed += OnPlayerDestroyed;

                    bool isLocalPlayer = player.SteamId == Steam.SteamId;
                    if(isLocalPlayer)
                    {
                        // TODO: remove when getting players entering/leaving controller
                        foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerListener>(FindMode.EverythingInSelfAndDescendants))
                            localPlayerInitializable.OnLocalPlayerCreated(player);
                    }
                }
            }
            return player;
        }
        return null;
    }

    private void OnPlayerDestroyed(Entity entity)
    {
        if(entity is not Player player)
            return;

        entity.Destroyed -= OnPlayerDestroyed;

        lock(_players)
        {
            if(object.ReferenceEquals(entity, LocalPlayer))
            {
                // TODO: remove when getting players entering/leaving controller
                foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerListener>(FindMode.EverythingInSelfAndDescendants))
                    localPlayerInitializable.OnLocalPlayerDestroyed(LocalPlayer);
            }

            if(object.ReferenceEquals(_players.GetValueOrDefault(player.SteamId), entity))
                _players[player.SteamId] = null;
        }
    }


    protected override void OnEnabled()
    {
        if(Instance.IsValid() && Instance != this)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(GameController)}. Destroying {this}...");
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

        if(InitializationStatus == InitializationStatus.NotInitialized)
            _ = Initialize();
    }

    protected override void OnUpdate()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        if(InitializationStatus != InitializationStatus.Initialized)
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
            Log.Warning($"{nameof(GameController)} was disabled. It's fine if it being destroyed or scene is unloading. Destroying {this} ...");
            Destroy();
        }
    }

    protected override void OnDestroy()
    {
        if(Instance.IsValid() && Instance != this)
            return;

        InitializationStatus = InitializationStatus.NotInitialized;
        LoadingStatus = LoadingStatus.NotLoaded;
        Instance = null;
    }

    private void AnimateBlockTextures()
    {
        BlocksTextureMap.UpdateAnimatedTextures();
    }

    public void RebuildBlockMeshes(IEnumerable<Block> blocks) =>
        RebuildBlockMeshes(blocks.SelectMany(block => block.BlockStateSet));

    public void RebuildBlockMeshes(IEnumerable<BlockState> blockStates)
    {
        foreach(var blockState in blockStates)
            BlockMeshes.Update(blockState);
    }

    public static bool TryCreateMod(TypeDescription type, out IMod mod)
    {
        var targetType = type.TargetType;
        if(!targetType.IsAssignableTo(typeof(IMod)))
        {
            mod = null!;
            return false;
        }

        if(targetType.IsAssignableTo(typeof(Component)))
        {
            GameObject modObject = new();
            mod = (modObject.Components.Create(type) as IMod)!;
            modObject.Name = mod.Id;
            return true;
        }

        try
        {
            mod = type.Create<IMod>();
            return true;
        }
        catch(MissingMethodException)
        {
            mod = default!;
            return false;
        }
    }

    public static bool TryCreateMod<T>(out T mod) where T : IMod
    {
        var created = TryCreateMod(TypeLibrary.GetType<T>(), out var createdMod);
        mod = created ? (T)createdMod : default!;
        return created;
    }

    public static bool TryCloneModFrom(GameObject modObject, out IMod mod) =>
        TryCloneModFrom<IMod>(modObject, out mod);

    public static bool TryCloneModFrom<T>(GameObject modObject, out T mod) where T : IMod
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
            throw new InvalidOperationException($"{nameof(GameController)} {this} is not valid");
        if(Instance.IsValid() && Instance != this)
            throw new InvalidOperationException($"{nameof(GameController)} {this} is duplicated instance");
        if(Scene.IsEditor)
            throw new InvalidOperationException($"{nameof(GameController)} {this} can't run in editor");
    }

    private void AssertInitializationStatus(InitializationStatus initializationStatus, bool equal = true)
    {
        AssertValid();
        bool statusEqual = InitializationStatus == initializationStatus;
        if(statusEqual != equal)
            throw new InvalidOperationException($"{nameof(GameController)}'s {nameof(InitializationStatus)} is{(equal ? string.Empty : " not")} {initializationStatus}");
    }

    private void AssertLoadingStatus(LoadingStatus loadingStatus, bool equal = true)
    {
        AssertValid();
        bool statusEqual = LoadingStatus == loadingStatus;
        if(statusEqual != equal)
            throw new InvalidOperationException($"{nameof(GameController)}'s {nameof(LoadingStatus)} is{(equal ? string.Empty : " not")} {loadingStatus}");
    }
}
