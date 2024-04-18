using Sandbox;
using VoxelWorld.Items;
using VoxelWorld.Meshing;
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
    public BlockItem WoodPlanks { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem TreeLeaves { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Furnace { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName, texturePath: $"{BaseMod.ModName}/items/tall_grass.png")]
    public BlockItem TallGrass { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Barrel { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName, texturePath: $"{BaseMod.ModName}/items/door.png")]
    public BlockItem Door { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Sand { get; private set; } = null!;

    [AutoBlockItem(BaseMod.ModName)]
    public BlockItem Workbench { get; private set; } = null!;

    public Item Stick { get; private set; } = new Item(MakeId("stick"),
        new Model[] { ItemFlatModelCreator.CreateModelFromMap($"{BaseMod.ModName}/items/stick.png") },
        Texture.Load(FileSystem.Mounted, $"textures/{BaseMod.ModName}/items/stick.png"), true);

    public DiggingItem WoodenPickaxe { get; private set; } = new(MakeId("wooden_pickaxe"),
        new Model[] { ItemFlatModelCreator.CreateModelFromMap($"{BaseMod.ModName}/items/wooden_pickaxe.png") },
        Texture.Load(FileSystem.Mounted, $"textures/{BaseMod.ModName}/items/wooden_pickaxe.png"), 1, true);

    public Item WoodenAxe { get; private set; } = new Item(MakeId("wooden_axe"),
        new Model[] { ItemFlatModelCreator.CreateModelFromMap($"{BaseMod.ModName}/items/wooden_axe.png") },
        Texture.Load(FileSystem.Mounted, $"textures/{BaseMod.ModName}/items/wooden_axe.png"), 1, true);
}
