

namespace Sandcube.Inventories;

public interface IStack<T> where T : class, IStack<T>
{
#pragma warning disable SB3000 // Hotloading not supported
    public abstract static T Empty { get; }
#pragma warning restore SB3000 // Hotloading not supported

    int Count { get; }
    bool IsEmpty => Count <= 0;
    int ValueStackLimit { get; }

    T WithCount(int count);

    bool EqualsValue(T other);
}

public static class IStackExtensions
{
    public static T Sub<T>(this IStack<T> stack, int count) where T : class, IStack<T>
    {
        return stack.WithCount(stack.Count - count);
    }

    public static T Add<T>(this IStack<T> stack, int count) where T : class, IStack<T>
    {
        return stack.WithCount(stack.Count + count);
    }
}
