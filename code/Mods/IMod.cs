using VoxelWorld.Registries;
using System.Threading.Tasks;
using VoxelWorld.Crafting.Recipes;

namespace VoxelWorld.Mods;

public interface IMod
{
    Id Id { get; }

    Task RegisterValues(RegistriesContainer registries) => Task.CompletedTask;
    void RegisterRecipes(RecipesContainer recipesContainer) { }

    void OnLoaded() { }
    void OnUnloaded() { }

    void OnGameLoaded() { }
}
