using VoxelWorld.Registries;

namespace VoxelWorld.Crafting.Recipes.Types;

public class RecipeType : IRegisterable
{
    public ModedId Id { get; }

    internal protected RecipeType(in ModedId id)
    {
        Id = id;
    }
}

public class RecipeType<TIn, TOut> : RecipeType
{
    public RecipeType(in ModedId id) : base(id)
    {
    }
}
