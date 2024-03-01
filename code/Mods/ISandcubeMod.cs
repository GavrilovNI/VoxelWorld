using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods;

public interface ISandcubeMod
{
    Id Id { get; }

    Task RegisterValues(RegistriesContainer registries) => Task.CompletedTask;

    void OnLoaded() { }
    void OnUnloaded() { }

    void OnGameLoaded() { }
}
