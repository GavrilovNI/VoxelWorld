using VoxelWorld.Items;
using VoxelWorld.Mods.Base;
using VoxelWorld.Registries;

namespace VoxelWorld.Mods.Base.Items;

public sealed class BaseModItems : ModItems
{
    private static ModedId MakeId(string itemId) => new(BaseMod.ModName, itemId);

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Stone { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem StoneSlab { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Dirt { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Grass { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Cobblestone { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Glass { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem WoodLog { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem TreeLeaves { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Furnace { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName, rawTexturePath: $"textures/{BaseMod.ModName}/items/tall_grass.png")]
    public BlockItem TallGrass { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Barrel { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName, rawTexturePath: $"textures/{BaseMod.ModName}/items/door.png")]
    public BlockItem Door { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Sand { get; private set; } = null!;
}
