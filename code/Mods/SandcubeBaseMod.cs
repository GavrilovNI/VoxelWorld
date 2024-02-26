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

    public async Task RegisterValues(RegistriesContainer registries)
    {
        RegistriesContainer container = new();

        Blocks = new();
        await Blocks.Register(registries);
        SandcubeGame.Instance!.RebuildBlockMeshes(registries.GetRegistry<Block>());
        Items = new();
        await Items.Register(registries);

        registries.Add(container);
    }

    public void OnGameLoaded()
    {
        SandcubeGame.Instance!.TryAddWorld(new ModedId(ModName, "main"), out _);
    }
}
