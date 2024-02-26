using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods;

public interface ISandcubeMod
{
    public Id Id { get; }

    public Task RegisterValues(RegistriesContainer registries) => Task.CompletedTask;

    public void OnLoaded();

    public void OnGameLoaded();
}
