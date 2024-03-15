using VoxelWorld.Interactions;
using VoxelWorld.Items;

namespace VoxelWorld.Inventories.Players;

public interface IHandsAccessor
{
    Stack<Item> GetHandItem(HandType handType);
    bool TrySetHandItem(HandType handType, Stack<Item> stack, bool simulate = false);
}
