

namespace Sandcube.Data;

public interface IPalette<T> : IReadOnlyPalette<T> where T : notnull
{
    int Count { get; }

    int GetOrAdd(T value);
    bool RemoveById(T value);
    bool RemoveByValue(int id);
    bool TryGetValue(int id, out T value);
    bool TryGetId(T value, out int id);
    T GetValue(int id);
    int GetId(T value);
}
