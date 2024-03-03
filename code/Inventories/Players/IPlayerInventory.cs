using Sandcube.IO;

namespace Sandcube.Inventories.Players;

public interface IPlayerInventory : IHotbarAccessor, IHandsAccessor, IBinaryWritable, IBinaryReadable
{
	public ItemStackInventory Main { get; }
	public ItemStackInventory SecondaryHand { get; }
	int MainHandIndex { get; set; }
}
