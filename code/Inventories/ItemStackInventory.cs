using Sandcube.IO;
using Sandcube.Items;
using System;
using System.IO;

namespace Sandcube.Inventories;

// TODO: remove when access T.Empty (Stack<Item>.Empty) will be whitelisted
[Obsolete("to remove when access T.Empty (Stack<Item>.Empty) will be whitelisted")]
public class ItemStackInventory : StackInventory<Stack<Item>>, IBinaryStaticReadable<ItemStackInventory>
{
    public ItemStackInventory(int size, int slotLimit = int.MaxValue) : base(size, slotLimit)
    {
    }

    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected sealed override Stack<Item> GetEmpty() => Stack<Item>.Empty;

    public static ItemStackInventory Read(BinaryReader reader)
    {
        int slotLimit = reader.ReadInt32();
        int size = reader.ReadInt32();
        ItemStackInventory result = new(size, slotLimit);
        for(int i = 0; i < size; ++i)
            result.Set(i, ItemStack.Read(reader));
        return result;
    }
}
