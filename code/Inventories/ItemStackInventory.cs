using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Items;
using System;

namespace VoxelWorld.Inventories;

// TODO: remove when access T.Empty (Stack<Item>.Empty) will be whitelisted
[Obsolete("to remove when access T.Empty (Stack<Item>.Empty) will be whitelisted")]
public class ItemStackInventory : StackInventory<Stack<Item>>, INbtStaticReadable<ItemStackInventory>
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
        int size = compoundTag.Get<int>("size");

        ItemStackInventory result = new(size, slotLimit);

        ListTag slotsTag = compoundTag.GetTag("slots").To<ListTag>();
        for(int i = 0; i < slotsTag.Count; ++i)
            result.Set(i, ItemStack.Read(slotsTag.GetTag(i)));

        return result;
    }
}
