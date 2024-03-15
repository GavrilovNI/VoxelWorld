using System.Collections.Generic;

namespace VoxelWorld.Data;

public interface IReadOnlyMultiValueBiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull where TValue : notnull
{
    int ValuesCount { get; }
    int KeysCount { get; }

    IEnumerable<TKey> Keys { get; }
    IEnumerable<TValue> Values { get; }

    TKey GetKey(TValue value);
    TKey GetKeyOrDefault(TValue value, TKey defaultKey);

    IReadOnlySet<TValue> GetValues(TKey key);
    IReadOnlySet<TValue> GetValuesOrDefault(TKey key, IReadOnlySet<TValue> defaultValues);

    bool TryGetKey(TValue value, out TKey key);
    bool TryGetValues(TKey key, out IReadOnlySet<TValue> values);

    bool ContainsKey(TKey key);
    bool ContainsValue(TValue value);

    public IReadOnlySet<TValue> this[TKey key] { get; }
    public TKey this[TValue key] { get; }
}

public static class IReadOnlyMultiValueBiDictionaryExtensions
{
    public static IReadOnlySet<TValue> GetValuesOrEmpty<TKey, TValue>(this IReadOnlyMultiValueBiDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : notnull =>
        dictionary.GetValuesOrDefault(key, new HashSet<TValue>());
}