

namespace Sandcube.Registries;

public interface IRegisterable : IModedIdProvider
{
    void OnRegistered();
}
