using Sandcube.Items;

namespace Sandcube.Inventories.Players;

public interface IHotbarAccessor
{
    int HotbarSize { get; }

    Stack<Item> GetHotbarItem(int index);
    bool TrySetHotbarItem(int index, Stack<Item> stack);
}
