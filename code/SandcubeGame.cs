using Sandbox;
using Sandcube.Events;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Blocks;
using Sandcube.Worlds.Generation;

namespace Sandcube;

public class SandcubeGame : BaseComponent, ISandcubeMod
{
    public const string ModName = "sandcube";

    public static SandcubeGame? Instance { get; private set; } = null;
    public static bool IsStarted => Instance?.Started ?? false;

    public Id Id { get; private set; } = new(ModName);

    public bool Started { get; private set; } = false;

    [Property] public World World { get; private set; } = null!;
    public Registry<Block> BlocksRegistry { get; private set; } = new ();
    public TextureMap TextureMap { get; private set; } = new ();
    public SandcubeBlocks Blocks { get; private set; } = new();
    public BlockMeshMap BlockMeshes { get; private set; } = new();

    public override void OnStart()
    {
        Event.Register(this);
        Instance = this;
        Prepare();

        Started = true;
        Event.Run(SandcubeEvent.Game.Start);
    }

    public override void OnDestroy()
    {
        if(Instance == this)
        {
            Event.Run(SandcubeEvent.Game.Stop);
            Started = false;
            Instance = null;
        }
        Event.Unregister(this);

    }

    protected virtual void Prepare()
    {
        RegisterAllBlocks();
        RebuildBlockMeshes();
    }

    [Event.Hotload]
    protected virtual void OnHotload()
    {
        Log.Info("Hotload");
        Instance = this;
        Prepare();
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

    public void RegisterBlocks(Registry<Block> registry)
    {
        Blocks.Register(registry);
    }
}
