using System;
using System.Collections.Generic;
using System.Linq;
using VoxelWorld.Crafting.Recipes.Types;
using VoxelWorld.Data;

namespace VoxelWorld.Crafting.Recipes;

public class RecipesContainer
{
    private readonly MultiValueDictionary<RecipeType, Recipe> _recipes = new();

    public void AddRecipe(Recipe recipe)
    {
        if(_recipes.ContainsValue(recipe.Type, recipe))
            throw new InvalidOperationException($"recipe already exists");
        _recipes.Add(recipe.Type, recipe);
    }

    public IEnumerable<Recipe> GetRecipes(RecipeType recipeType)
    {
        if(_recipes.TryGetValues(recipeType, out var recipes))
            return recipes;
        return Enumerable.Empty<Recipe>();
    }
}
