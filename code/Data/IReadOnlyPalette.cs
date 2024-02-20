

namespace Sandcube.Data;

public interface IReadOnlyPalette<T> where T : notnull
{
    int Count { get; }

    bool TryGetValue(int id, out T value);
    bool TryGetId(T value, out int id);
    T GetValue(int id);
    int GetId(T value);
}
