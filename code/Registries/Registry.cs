using Sandcube.Data.Enumarating;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Registries;

public abstract class Registry
{
    private readonly Dictionary<ModedId, IRegisterable> _values = new();
    public IReadOnlyDictionary<ModedId, IRegisterable> All => _values.AsReadOnly();

    public Type ValueType { get; }

    protected internal Registry(Type valueType)
    {
        ValueType = valueType;
    }

    public abstract Registry Clone();

    public void Clear() => _values.Clear();

    public void Register(IRegisterable value)
    {
        if(!value.GetType().IsAssignableTo(ValueType))
            throw new InvalidOperationException($"Can't register value of type {value.GetType()}");

        if(_values.ContainsKey(value.Id))
            throw new InvalidOperationException($"Value with {nameof(ModedId)} '{value.Id}' was already registered");

        _values.Add(value.Id, value);
    }

    public void RegisterAll(IEnumerable<IRegisterable> values)
    {
        foreach(var value in values)
            Register(value);
    }

    public IRegisterable? Get(ModedId id) => _values!.GetValueOrDefault(id, null);

    public bool CanRegister(Type type) => type.IsAssignableTo(ValueType);
    public bool CanRegister<T>() => typeof(T).IsAssignableTo(ValueType);

    public IEnumerator<IRegisterable> GetEnumerator() => All.Values.GetEnumerator();
}

public class Registry<T> : Registry, IEnumerable<T> where T : class, IRegisterable
{
    public new IReadOnlyDictionary<ModedId, T> All => base.All.ToDictionary(kv => kv.Key, kv => (T)kv.Value).AsReadOnly();

    public Registry() : base(typeof(T))
    {
    }

    public override Registry Clone()
    {
        var registry = new Registry<T>();
        foreach(var kv in All)
            registry.Register(kv.Value);
        return registry;
    }

    public void Register(T value) => base.Register(value);

    public new T? Get(ModedId id) => base.Get(id) as T;

    public new IEnumerator<T> GetEnumerator() => base.GetEnumerator().Cast<T>();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
