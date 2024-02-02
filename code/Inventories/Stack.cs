using System;

namespace Sandcube.Inventories;

public record class Stack<T> : IStack<Stack<T>> where T : class, IStackValue
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
        if(count == Count || count < 0 && Count == 0 || Value is null)
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
}
