using System.Collections.Generic;

namespace Sandcube.Data.Enumarating;

public static class IEnumeratorExtensions
{
    public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator) => enumerator;
}
