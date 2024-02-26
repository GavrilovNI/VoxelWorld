using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods;

public sealed class SandcubeBaseMod : ISandcubeMod
{
    public const string ModName = "sandcube";
    public static SandcubeBaseMod? Instance => SandcubeGame.Instance?.BaseMod;

    public Id Id { get; } = new(ModName);

    public SandcubeBlocks Blocks { get; private set; } = null!;
    public SandcubeItems Items { get; private set; } = null!;

    public void OnLoaded() { }

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

    public void OnGameLoaded()
    {
        SandcubeGame.Instance!.TryAddWorld(new ModedId(ModName, "main"), out _);
    }
}
