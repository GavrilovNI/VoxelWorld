using Sandbox;
using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation.Meshes;
using System;
using System.Threading.Tasks;

namespace Sandcube;

public class SandcubeGame : Component, ISandcubeMod
{
    public const string ModName = "sandcube";

    public static event Action? Started;
    public static event Action? Stopped;
    public static bool IsStarted { get; protected set; } = false;

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
    [Property] public BlockPhotoMaker BlockPhotoMaker { get; private set; } = null!;

    public Registry<Block> BlocksRegistry { get; private set; } = new();
    public Registry<Item> ItemsRegistry { get; private set; } = new();
    public TextureMap TextureMap { get; private set; } = new();
    public SandcubeBlocks Blocks { get; private set; } = null!;
    public SandcubeItems Items { get; private set; } = null!;
    public BlockMeshMap BlockMeshes { get; } = new();


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

    protected virtual async Task OnInitialize()
    {
        await RegisterAllBlocks();
        RebuildBlockMeshes();
        await RegisterAllItems();

        if(!Enabled || !this.IsValid())
            return;

        IsStarted = true;
        Started?.Invoke();
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

    private async Task RegisterAllBlocks()
    {
        BlocksRegistry.Clear();
        await RegisterBlocks(BlocksRegistry);
    }

    private async Task RegisterAllItems()
    {
        ItemsRegistry.Clear();
        await RegisterItems(ItemsRegistry);
    }

    public async Task RegisterBlocks(Registry<Block> registry)
    {
        Blocks = new();
        await Blocks.Register(registry);
    }

    public async Task RegisterItems(Registry<Item> registry)
    {
        Items = new();
        await Items.Register(registry);
    }
}
