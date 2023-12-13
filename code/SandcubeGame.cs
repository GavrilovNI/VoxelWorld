using Sandbox;
using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation.Meshes;
using System;

namespace Sandcube;

public class SandcubeGame : Component, ISandcubeMod
{
    public const string ModName = "sandcube";

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

    public Id Id { get; private set; } = new(ModName);

    [Property] public World World { get; private set; } = null!;
    public Registry<Block> BlocksRegistry { get; private set; } = new ();
    public Registry<Item> ItemsRegistry { get; private set; } = new ();
    public TextureMap TextureMap { get; private set; } = new ();
    public SandcubeBlocks Blocks { get; private set; } = new();
    public SandcubeItems Items { get; private set; } = new();
    public BlockMeshMap BlockMeshes { get; private set; } = new();

    protected override void OnAwake()
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
        Prepare();
        IsStarted = true;
        Started?.Invoke();
    }

    protected override void OnDisabled()
    {
        if(Instance != this)
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

    protected virtual void Prepare()
    {
        RegisterAllBlocks();
        RegisterAllItems();
        RebuildBlockMeshes();
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

    private void RegisterAllBlocks()
    {
        BlocksRegistry.Clear();
        RegisterBlocks(BlocksRegistry);
    }

    private void RegisterAllItems()
    {
        ItemsRegistry.Clear();
        RegisterItems(ItemsRegistry);
    }

    public void RegisterBlocks(Registry<Block> registry)
    {
        Blocks.Register(registry);
    }

    public void RegisterItems(Registry<Item> registry)
    {
        Items.Register(registry);
    }
}
