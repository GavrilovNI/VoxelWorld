using Sandcube.Registries;

namespace Sandcube.Inventories;

public interface IStackValue
{
    int StackLimit { get; }
    ModedId Id { get; }
}
