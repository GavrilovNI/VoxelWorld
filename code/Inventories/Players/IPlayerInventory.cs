using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor
{
	public IIndexedCapability<Stack<Item>> Main { get; }
	int MainHandIndex { get; set; }
}
