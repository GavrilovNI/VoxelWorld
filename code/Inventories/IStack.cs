using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using System;

namespace Sandcube.Inventories;

//TODO: implement IBinaryStaticReadable<T> when T.Read get whitelisted
//TODO: implement INbtStaticReadable<T> when T.Read get whitelisted
public interface IStack<T> : INbtWritable, IBinaryWritable where T : class, IStack<T>
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
    public static T Subtract<T>(this IStack<T> stack, int count) where T : class, IStack<T>
    {
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "can't be less than 0");

        return stack.WithCount(stack.Count - count);
    }

    public static T Add<T>(this IStack<T> stack, int count) where T : class, IStack<T>
    {
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "can't be less than 0");

        return stack.WithCount(stack.Count + count);
    }
}
