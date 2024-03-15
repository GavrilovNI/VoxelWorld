using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods;

public interface IMod
{
    Id Id { get; }

    Task RegisterValues(RegistriesContainer registries) => Task.CompletedTask;

    void OnLoaded() { }
    void OnUnloaded() { }

    void OnGameLoaded() { }
}
