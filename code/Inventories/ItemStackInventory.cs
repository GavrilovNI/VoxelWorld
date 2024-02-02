using Sandcube.Items;
using System;

namespace Sandcube.Inventories;

// TODO: remove when access T.Empty (Stack<Item>.Empty) will be whitelisted
[Obsolete("to remove when access T.Empty (Stack<Item>.Empty) will be whitelisted")]
internal class ItemStackInventory : StackInventory<Stack<Item>>
{
    public ItemStackInventory(int size, int slotLimit = int.MaxValue) : base(size, slotLimit)
    {
    }

    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected sealed override Stack<Item> GetEmpty() => Stack<Item>.Empty;
}
