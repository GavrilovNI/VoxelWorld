using System;
using VoxelWorld.Crafting.Recipes.Types;
using VoxelWorld.Registries;

namespace VoxelWorld.Crafting.Recipes;

public abstract class Recipe
{
    public RecipeType Type { get; }
    public ModedId Id { get; }

    public Recipe(RecipeType recipeType, in ModedId id)
    {
        Type = recipeType;
        Id = id;
    }

    public static bool operator ==(Recipe left, Recipe right) => left.Type == right.Type && left.Id == right.Id;
    public static bool operator !=(Recipe left, Recipe right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(Type, Id);

    public override bool Equals(object? obj) => obj is Recipe other && this == other;
}

public abstract class Recipe<TIn, TOut> : Recipe
{
    public Recipe(RecipeType recipeType, in ModedId id) : base(recipeType, id)
    {
    }

    public abstract bool Matches(TIn input);
    public abstract TOut Process(in TIn input);
    public abstract TOut GetResult();
}
