using VoxelWorld.Registries;
using System.Threading.Tasks;

namespace VoxelWorld.Mods;

public interface IMod
{
    Id Id { get; }

    Task RegisterValues(RegistriesContainer registries) => Task.CompletedTask;

    void OnLoaded() { }
    void OnUnloaded() { }

    void OnGameLoaded() { }
}
