using Sandcube.Items;
using Sandcube.Mods.Base;
using Sandcube.Registries;

namespace Sandcube.Mods.Base.Items;

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
    public BlockItem Grass { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Cobblestone { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Glass { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem WoodLog { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem TreeLeaves { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Furnace { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName, rawTexturePath: $"textures/{SandcubeBaseMod.ModName}/items/tall_grass.png")]
    public BlockItem TallGrass { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Barrel { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName, rawTexturePath: $"textures/{SandcubeBaseMod.ModName}/items/door.png")]
    public BlockItem Door { get; private set; } = null!;

    [AutoBlockItem(SandcubeBaseMod.ModName)]
    public BlockItem Sand { get; private set; } = null!;
}
