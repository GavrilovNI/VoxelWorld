

using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor
{
	public IIndexedCapability<Stack<Item>> HotBar { get; }
	int MainHandIndex { get; set; }
}
