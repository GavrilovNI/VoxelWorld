using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sandcube.Registries;

public class Registry<T> where T : class, IRegisterable
{
    private readonly Dictionary<ModedId, T> _blocks = new();
    public ReadOnlyDictionary<ModedId, T> All => _blocks.AsReadOnly();

    public void Clear() => _blocks.Clear();

    public void Add(T value)
    {
        if(_blocks.ContainsKey(value.Id))
            throw new InvalidOperationException($"{typeof(T).Name} with {nameof(ModedId)} '{value.Id}' already registered!");
        _blocks.Add(value.Id, value);
        value.OnRegistered();
    }

    public T? Get(ModedId id) => _blocks!.GetValueOrDefault(id, null);
}
