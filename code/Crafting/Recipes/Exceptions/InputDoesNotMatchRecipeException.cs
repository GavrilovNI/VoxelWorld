using System;

namespace VoxelWorld.Crafting.Recipes.Exceptions;

public class InputDoesNotMatchRecipeException<TIn> : Exception
{
    public InputDoesNotMatchRecipeException(TIn input, Recipe recipe) : base($"{input} doesn't match recipe {recipe.Id}")
    {

    }

    public static void ThrowIfDoesNotMatchRecipe<TOut>(TIn input, Recipe<TIn, TOut> recipe)
    {
        if(!recipe.Matches(input))
            throw new InputDoesNotMatchRecipeException<TIn>(input, recipe);
    }
}
