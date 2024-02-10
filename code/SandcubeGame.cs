using Sandbox;
using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Mods;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation.Meshes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube;

public sealed class SandcubeGame : Component
{
    public static event Action? Started;
    public static event Action? Stopped;
    public static bool IsStarted { get; private set; } = false;

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

    [Property] public World World { get; private set; } = null!;
    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;
    [Property] public bool ShouldAnimateBlockTextures { get; private set; } = true;

    public Registry<Block> BlocksRegistry { get; } = new();
    public Registry<Item> ItemsRegistry { get; } = new();
    public PathedTextureMap BlocksTextureMap { get; } = new("textures/", new Color32(255, 0, 255));
    public BlockMeshMap BlockMeshes { get; } = new();

    public SandcubeBaseMod BaseMod { get; private set; } = null!;

    private readonly Dictionary<Id, ISandcubeMod> _mods = new();

    protected override void OnEnabled()
    {
        if(Instance.IsValid() && Instance != this)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(SandcubeGame)}. Destroying this...");
            Destroy();
            return;
        }

        if(!Scene.IsEditor)
            Instance = this;
    }

    protected override void OnStart()
    {
        if(Instance != this)
            return;

        _ = OnInitialize();
    }

    protected override void OnDisabled()
    {
        if(Instance != this || !IsStarted)
            return;

        Stopped?.Invoke();
        IsStarted = false;
    }

    protected override void OnDestroy()
    {
        if(Instance != this)
            return;

        Instance = null;
    }

    private async Task OnInitialize()
    {
        await LoadAllMods();

        if(!Enabled || !this.IsValid())
            return;

        IsStarted = true;
        Started?.Invoke();
    }

    protected override void OnUpdate()
    {
        if(!IsStarted)
            return;

        if(ShouldAnimateBlockTextures)
            AnimateBlockTextures();
    }

    private void AnimateBlockTextures()
    {
        if(BlocksTextureMap.UpdateAnimatedTextures())
            World.UpdateTexture(BlocksTextureMap.Texture);
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

    public async Task LoadMods(IEnumerable<ISandcubeMod> mods)
    {
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
    }

    public async Task LoadMod(ISandcubeMod mod)
    {
        if(_mods.ContainsKey(mod.Id))
            throw new InvalidOperationException($"Mod with id {mod.Id} was already added");
        _mods[mod.Id] = mod;

        await mod.RegisterBlocks(BlocksRegistry);
        RebuildBlockMeshes();
        await mod.RegisterItems(ItemsRegistry);
        mod.OnLoaded();
    }

    public ISandcubeMod? GetMod(Id id) => _mods!.GetValueOrDefault(id, null);
    public bool IsModLoaded(Id id) => _mods.ContainsKey(id);
}
