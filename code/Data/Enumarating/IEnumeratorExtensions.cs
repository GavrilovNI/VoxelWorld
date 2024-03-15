using System.Collections;
using System.Collections.Generic;

namespace VoxelWorld.Data.Enumarating;

public static class IEnumeratorExtensions
{
    public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator) => enumerator;

    public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
    {
        while(enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static IEnumerator<T> Cast<T>(this IEnumerator enumerator)
    {
        while(enumerator.MoveNext())
            yield return (T)enumerator.Current;
    }
}
