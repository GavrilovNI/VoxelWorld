

namespace VoxelWorld.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor
{
	public ItemStackInventory Main { get; }
	public ItemStackInventory SecondaryHand { get; }
	int MainHandIndex { get; set; }
}
