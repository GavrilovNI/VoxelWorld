using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Data;

public class MultiValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, List<TValue>> _values = new();

    public int Count { get; private set; } = 0;

    public IEnumerable<TKey> Keys => _values.Keys;
    public IEnumerable<TValue> Values
    {
        get
        {
            foreach(var (_, values) in _values)
            {
                foreach(var value in values)
                    yield return value;
            }
        }
    }

    public MultiValueDictionary()
    {

    }

    public void Clear()
    {
        _values.Clear();
        Count = 0;
    }

    public void Add(TKey key, TValue value)
    {
        GetOrCreateValues(key).Add(value);
        Count++;
    }

    public IReadOnlyList<TValue> GetValues(TKey key)
    {
        if(_values.TryGetValue(key, out var values))
            return values;
        throw new KeyNotFoundException($"key {key} not found");
    }

    public IReadOnlyList<TValue> GetValuesOrDefault(TKey key, IReadOnlyList<TValue> defaultValues)
    {
        if(_values.TryGetValue(key, out var values))
            return values;
        return defaultValues;
    }

    public bool TryGetValues(TKey key, out IReadOnlyList<TValue> values)
    {
        if(_values.TryGetValue(key, out var result))
        {
            values = result;
            return true;
        }
        values = null!;
        return false;
    }

    public bool ContainsKey(TKey key) => _values.ContainsKey(key);
    public bool ContainsValue(TValue value) => _values.Any(kv => kv.Value.Contains(value));
    public bool ContainsValue(TKey key, TValue value) => _values.TryGetValue(key, out var values) && values.Contains(value);

    public bool RemoveValue(TKey key, TValue value)
    {
        if(!_values.TryGetValue(key, out var values))
            return false;

        bool removed = values.Remove(value);
        Count--;

        if(values.Count == 0)
            _values.Remove(key);

        return removed;
    }

    public bool RemoveAllValues(TKey key)
    {
        if(!_values.TryGetValue(key, out var values))
            return false;

        Count -= values.Count;
        _values.Remove(key);
        return true;
    }

    private List<TValue> GetOrCreateValues(TKey key)
    {
        if(_values.TryGetValue(key, out var values))
            return values;

        var result = new List<TValue>();
        _values[key] = result;
        return result;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach(var (key, values) in _values)
        {
            foreach(var value in values)
                yield return KeyValuePair.Create(key, value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IReadOnlyList<TValue> this[TKey key] => _values[key];
}
