using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IHotbarAccessor
{
    ItemStackInventory Hotbar { get; }
}
