

namespace Sandcube.Data;

public interface IPalette<T> : IReadOnlyPalette<T> where T : notnull
{
    int GetOrAdd(T value);
    bool RemoveById(T value);
    bool RemoveByValue(int id);
}
