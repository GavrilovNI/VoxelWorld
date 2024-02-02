

namespace Sandcube.Registries;

public interface IModedIdAccessor : IModedIdProvider
{
    new ModedId Id { get; set; }
}
