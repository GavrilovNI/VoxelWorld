

namespace Sandcube.Inventories;

public interface IStack<T> where T : class, IStack<T>
{
#pragma warning disable SB3000 // Hotloading not supported
    public abstract static T Empty { get; }
#pragma warning restore SB3000 // Hotloading not supported

    int Count { get; }
    bool IsEmpty => Count <= 0;

    T WithCount(int count);

    bool EqualsValue(T other);
}
