using VoxelWorld.Data.Enumarating;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Registries;

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

    public bool Contains(ModedId id) => _values.ContainsKey(id);
    public IRegisterable Get(ModedId id) => _values[id];

    public bool TryGet(ModedId id, out IRegisterable registerable) => _values.TryGetValue(id, out registerable!);

    public bool CanRegister(Type type) => type.IsAssignableTo(ValueType);
    public bool CanRegister<T>() => typeof(T).IsAssignableTo(ValueType);

    public IEnumerator<IRegisterable> GetEnumerator() => All.Values.GetEnumerator();
}

public class Registry<T> : Registry, IEnumerable<T> where T : IRegisterable
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

    public new T Get(ModedId id) => (T)base.Get(id);

    public bool TryGet(ModedId id, out T value)
    {
        if(base.TryGet(id, out var registerable))
        {
            value = (T)registerable;
            return true;
        }

        value = default!;
        return false;
    }

    public new IEnumerator<T> GetEnumerator() => base.GetEnumerator().Cast<T>();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
