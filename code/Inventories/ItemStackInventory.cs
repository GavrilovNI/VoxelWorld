using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Items;
using System;
using System.Drawing;
using System.IO;

namespace Sandcube.Inventories;

// TODO: remove when access T.Empty (Stack<Item>.Empty) will be whitelisted
[Obsolete("to remove when access T.Empty (Stack<Item>.Empty) will be whitelisted")]
public class ItemStackInventory : StackInventory<Stack<Item>>, INbtStaticReadable<ItemStackInventory>, IBinaryStaticReadable<ItemStackInventory>
{
    public ItemStackInventory(int size, int slotLimit = int.MaxValue) : base(size, slotLimit)
    {
    }

    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected sealed override Stack<Item> GetEmpty() => Stack<Item>.Empty;

    public static ItemStackInventory Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        int slotLimit = compoundTag.Get<int>("slot_limit");

        ListTag slotsTag = compoundTag.GetTag("slots").To<ListTag>();
        ItemStackInventory result = new(slotsTag.Count, slotLimit);

        for(int i = 0; i < slotsTag.Count; ++i)
            result.Set(i, ItemStack.Read(slotsTag.GetTag(i)));

        return result;
    }

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
