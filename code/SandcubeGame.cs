using Sandbox;
using Sandcube.Events;
using Sandcube.Mth;
using Sandcube.Worlds;
using Sandcube.Worlds.Blocks;
using Sandcube.Worlds.Generation;

namespace Sandcube;

public class SandcubeGame : BaseComponent, ISandcubeMod
{
    public const string ModName = "sandcube";

    public static SandcubeGame? Instance { get; private set; } = null;
    public static bool Started => Instance != null;

    public Id Id { get; private set; } = new(ModName);

    [Property] public World World { get; private set; } = null!;
    public BlocksRegistry BlocksRegistry { get; private set; } = new ();
    public TextureMap TextureMap { get; private set; } = new ();
    public SandcubeBlocks Blocks { get; private set; } = new();
    public BlockMeshMap BlockMeshes { get; private set; } = new();

    private bool _blockMeshesRebuildRequiered;

    public override void OnStart()
    {
        Event.Register(this);
        Instance = this;
        Event.Run(SandcubeEvent.Game.Start);
        _blockMeshesRebuildRequiered = true;
        RegisterAllBlocks();
    }

    public override void OnDestroy()
    {
        if(Instance == this)
        {
            Event.Run(SandcubeEvent.Game.Stop);
            Instance = null;
        }
        Event.Unregister(this);

    }

    [Event.Hotload]
    protected virtual void OnHotload()
    {
        Log.Info("Hotload");
        Instance = this;
        Id = new(ModName);
        RegisterAllBlocks();
    }

    protected void RegisterAllBlocks()
    {
        var oldTextureMapSize = TextureMap.Texture.Size;

        BlocksRegistry.Clear();
        RegsiterBlocks(BlocksRegistry);

        _blockMeshesRebuildRequiered |= oldTextureMapSize != TextureMap.Texture.Size;

        if(_blockMeshesRebuildRequiered)
        {
            BlockMeshes.Clear();
            foreach(var block in BlocksRegistry.All)
                BlockMeshes.Add(block.Value);
        }
    }

    public void RegsiterBlocks(BlocksRegistry registry)
    {
        Blocks.Register(registry);
    }
}
