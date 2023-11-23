using Sandcube.Interactions;
using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IHandsAccessor
{
    Stack<Item> GetHandItem(HandType handType);
    bool TrySetHandItem(HandType handType, Stack<Item> stack);
}
