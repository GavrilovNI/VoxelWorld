using Sandcube.Interactions;
using Sandcube.Items;
using System;

namespace Sandcube.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor
{
	public IIndexedCapability<Stack<Item>> Main { get; }
	public IIndexedCapability<Stack<Item>> SecondaryHand { get; }
	int MainHandIndex { get; set; }
}
