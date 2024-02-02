using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor
{
	int MainHandIndex { get; set; }
}
