using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IHotbarAccessor
{
    IIndexedCapability<Stack<Item>> Hotbar { get; }
}
