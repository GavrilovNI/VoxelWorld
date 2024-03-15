using VoxelWorld.Items;

namespace VoxelWorld.Inventories.Players;

public interface IHotbarAccessor
{
    ItemStackInventory Hotbar { get; }
}
