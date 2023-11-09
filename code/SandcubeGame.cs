using Sandbox;
using Sandcube.Events;
using Sandcube.Mth;
using Sandcube.Worlds;
using Sandcube.Worlds.Blocks;

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


    [Property] public bool Test { get; set; } = false;
    [Property] public bool Test2 { get; set; } = false;

    public SandcubeBlocks Blocks { get; private set; } = new();


    public override void Update()
    {
        if(Test)
        {
            Test = false;
            for(int x = -2; x <= 2; ++x)
            {
                for(int y = -2; y <= 2; ++y)
                {
                    for(int z = 0; z <= 10; ++z)
                    {
                        Vector3Int position = new(x, y, z);
                        World.GenerateChunk(position);
                    }
                }
            }
        }
        if(Test2)
        {
            Test2 = false;
            World.Clear();
        }
    }

    public override void OnStart()
    {
        Event.Register(this);
        Instance = this;
        Event.Run(SandcubeEvent.Game.Start);
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
        BlocksRegistry.Clear();
        RegsiterBlocks(BlocksRegistry);
    }

    public void RegsiterBlocks(BlocksRegistry registry)
    {
        Blocks.Register(registry);
    }
}
