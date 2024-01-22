using Sandcube.Mods;
using Sandcube.Registries;

namespace Sandcube.Items;

public sealed class SandcubeItems : ModItems
{
    private static ModedId MakeId(string itemId) => new(SandcubeBaseMod.ModName, itemId);

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Stone { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem StoneSlab { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Dirt { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Cobblestone { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Glass { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem WoodLog { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Furnace { get; private set; } = null!;
}
