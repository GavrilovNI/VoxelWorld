using System;

namespace Sandcube.Registries;

public interface IModedIdAccessor : IModedIdProvider
{
    new ModedId ModedId { get; set; }
}
