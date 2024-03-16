using VoxelWorld.Crafting.Recipes.Types;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Registries;

namespace VoxelWorld.Mods.Base.Recipes;

public sealed class BaseRecipeTypes : ModRegisterables<RecipeType>
{
    private static ModedId MakeId(string blockId) => new(BaseMod.ModName, blockId);

    public RecipeType<IIndexedCapability<Stack<Item>>, Stack<Item>> Workbench { get; } = new(MakeId("workbench"));
}
