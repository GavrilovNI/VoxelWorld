using System;
using System.Collections;
using System.Collections.Generic;

namespace VoxelWorld.Data;

public class MultiValueBiDictionary<TKey, TValue> : IReadOnlyMultiValueBiDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
{
    private readonly Dictionary<TKey, HashSet<TValue>> _keyToValues = new();
    private readonly Dictionary<TValue, TKey> _valueToKey = new();

    public int ValuesCount => _valueToKey.Count;
    public int KeysCount => _keyToValues.Count;

    public IEnumerable<TKey> Keys => _valueToKey.Values;
    public IEnumerable<TValue> Values => _valueToKey.Keys;

    public MultiValueBiDictionary()
    {

    }

    public MultiValueBiDictionary(IReadOnlyMultiValueBiDictionary<TKey, TValue> other)
    {
        foreach(var (key, value) in other)
            Add(key, value);
    }

    public void Clear()
    {
        _keyToValues.Clear();
        _valueToKey.Clear();
    }

    public int GetValuesCount(TKey key) => TryGetValues(key, out var values) ? values.Count : 0;

    public void Add(TKey key, TValue value)
    {
        if(_valueToKey.ContainsKey(value))
            throw new ArgumentException($"{value} was already presented in dictionary", nameof(value));

        _valueToKey[value] = key;
        GetOrCreateValues(key).Add(value);
    }

    public void Set(TKey key, IEnumerable<TValue> values)
    {
        RemoveAllValues(key);
        var valuesSet = GetOrCreateValues(key);
        foreach(var value in values)
        {
            valuesSet.Add(value);
            _valueToKey[value] = key;
        }
        if(valuesSet.Count == 0)
            _keyToValues.Remove(key);
    }

    public TKey GetKey(TValue value)
    {
        if(_valueToKey.TryGetValue(value, out var key))
            return key;
        throw new KeyNotFoundException($"value {value} not found");
    }

    public TKey GetKeyOrDefault(TValue value, TKey defaultKey) => _valueToKey.GetValueOrDefault(value, defaultKey);

    public IReadOnlySet<TValue> GetValues(TKey key)
    {
        if(_keyToValues.TryGetValue(key, out var values))
            return values;
        throw new KeyNotFoundException($"key {key} not found");
    }

    public IReadOnlySet<TValue> GetValuesOrDefault(TKey key, IReadOnlySet<TValue> defaultValues)
    {
        if(_keyToValues.TryGetValue(key, out var values))
            return values;
        return defaultValues;
    }

    public bool TryGetKey(TValue value, out TKey key) => _valueToKey.TryGetValue(value, out key!);
    public bool TryGetValues(TKey key, out IReadOnlySet<TValue> values)
    {
        if(_keyToValues.TryGetValue(key, out var result))
        {
            values = result;
            return true;
        }
        values = null!;
        return false;
    }

    public bool ContainsKey(TKey key) => _keyToValues.ContainsKey(key);
    public bool ContainsValue(TValue value) => _valueToKey.ContainsKey(value);

    public bool RemoveValue(TValue value)
    {
        if(!_valueToKey.TryGetValue(value, out var key))
            return false;

        _valueToKey.Remove(value);
        var values = GetOrCreateValues(key);
        values.Remove(value);
        if(values.Count == 0)
            _keyToValues.Remove(key);
        return true;
    }

    public bool RemoveAllValues(TKey key)
    {
        if(!_keyToValues.TryGetValue(key, out var values))
            return false;

        foreach(var value in values)
            _valueToKey.Remove(value);
        _keyToValues.Remove(key);

        return true;
    }

    private HashSet<TValue> GetOrCreateValues(TKey key)
    {
        if(_keyToValues.TryGetValue(key, out var values))
            return values;

        var result = new HashSet<TValue>();
        _keyToValues[key] = result;
        return result;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach(var (value, key) in _valueToKey)
            yield return KeyValuePair.Create(key, value);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IReadOnlySet<TValue> this[TKey key] => _keyToValues[key];

    public TKey this[TValue key]
    {
        get => _valueToKey[key];
        set
        {
            RemoveValue(key);
            Add(value, key);
        }
    }
}
