using Sandbox;
using Sandcube.Blocks;
using Sandcube.Mods.Base.Blocks;
using Sandcube.Mods.Base.Blocks.Entities;
using Sandcube.Mods.Base.Entities;
using Sandcube.Mods.Base.Items;
using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods.Base;

public sealed class BaseMod : Component, IMod
{
    public const string ModName = "sandcube";
    public static BaseMod? Instance { get; private set; }

    public Id Id { get; } = new(ModName);

    public BaseModBlocks Blocks { get; private set; } = null!;
    public BaseModBlockEntities BlockEntities { get; private set; } = null!;
    public BaseModItems Items { get; private set; } = null!;
    public BaseModEntities Entities { get; private set; } = null!;

    private readonly ModedId _mainWorldId = new(ModName, "main");

    protected override void OnAwake()
    {
        if(Instance is not null)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(GameController)}. Destroying {this}...");
            Destroy();
            return;
        }

        Instance = this;

        Blocks = new();
        BlockEntities = new();
        Items = new();
        Entities = Components.Get<BaseModEntities>(true);
    }

    protected override void OnDestroy()
    {
        if(Instance != this)
            return;

        Instance = null;
    }

    public async Task RegisterValues(RegistriesContainer registries)
    {
        RegistriesContainer container = new();

        await Blocks.Register(registries);
        await BlockEntities.Register(registries);
        GameController.Instance!.RebuildBlockMeshes(registries.GetRegistry<Block>());
        await Items.Register(registries);
        await Entities.Register(registries);

        registries.Add(container);
    }

    public void OnGameLoaded()
    {
        GameController.Instance!.TryAddWorld(_mainWorldId, out _);
    }
}
