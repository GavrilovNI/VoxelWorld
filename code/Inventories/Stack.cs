using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Items;
using System;

namespace VoxelWorld.Inventories;

public record class Stack<T> : IStack<Stack<T>> where T : class, IStackValue<T>
{
#pragma warning disable SB3000 // Hotloading not supported
    public static Stack<T> Empty { get; } = new(null!, 0);
#pragma warning restore SB3000 // Hotloading not supported

    public T? Value { get; private init; }
    public int Count { get; private init; }
    public int ValueStackLimit => Value?.StackLimit ?? 0;

    public bool IsEmpty => Count <= 0;

    public Stack(T value, int count = 1)
    {
        Value = value;
        Count = Value is null ? 0 : Math.Max(0, count);
    }

    public Stack<T> WithCount(int count)
    {
        if(count <= 0 || Value is null)
            return Empty;

        if(count == Count)
            return this;

        return new Stack<T>(Value, count);
    }

    public bool EqualsValue(Stack<T> other)
    {
        if(IsEmpty)
            return other.IsEmpty;

        if(other.IsEmpty)
            return false;

        return Value!.Equals(other.Value);
    }

    public override int GetHashCode() => HashCode.Combine(Value, Count);

    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("count", Count);
        if(Count > 0)
            tag.Set("value", Value!);
        return tag;
    }
}

public record class ItemStack : Stack<Item>, INbtStaticReadable<ItemStack>
{
#pragma warning disable SB3000 // Hotloading not supported
    public static new ItemStack Empty { get; } = new(null!, 0);
#pragma warning restore SB3000 // Hotloading not supported

    public ItemStack(Item value, int count = 1) : base(value, count)
    {
    }

    public static ItemStack Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        int count = compoundTag.Get<int>("count");
        if(count <= 0)
            return Empty;

        var item = Item.Read(compoundTag.GetTag("value"));
        return new(item, count);
    }
}
