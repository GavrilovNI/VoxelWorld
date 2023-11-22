using Sandcube.Items;

namespace Sandcube.Inventories;

// TODO: remove when access T.Empty (Stack<Item>.Empty) will be whitelisted
[Obsolete("to remove when access T.Empty (Stack<Item>.Empty) will be whitelisted")]
internal class ItemStackInventory : StackInventory<Stack<Item>>
{
    public ItemStackInventory(int size) : base(size)
    {
    }

    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected sealed override Stack<Item> GetEmpty() => Stack<Item>.Empty;
}
