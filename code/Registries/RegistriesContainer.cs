using Sandcube.Data.Enumarating;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Registries;

public class RegistriesContainer : IEnumerable<KeyValuePair<Type, Registry>>
{
    private readonly Dictionary<Type, Registry> _registries = new();

    public void Clear() => _registries.Clear();

    public Registry<T> GetOrAddRegistry<T>() where T : IRegisterable
    {
        var type = typeof(T);
        if(_registries.TryGetValue(type, out var registry))
            return (Registry<T>)registry;

        var result = new Registry<T>();
        _registries[type] = result;
        return result;
    }

    public Registry<T> GetRegistry<T>() where T : IRegisterable => (Registry<T>)_registries[typeof(T)];

    public bool TryGetRegistry<T>(out Registry<T> registry) where T : IRegisterable
    {
        if(_registries.TryGetValue(typeof(T), out var result))
        {
            registry = (Registry<T>)result;
            return true;
        }

        registry = null!;
        return false;
    }

    public void Register<T>(IRegisterable value) where T : IRegisterable
    {
        GetOrAddRegistry<T>().Register(value);
    }

    public void Add(RegistriesContainer container)
    {
        foreach(var (type, registry) in container._registries)
        {
            if(_registries.TryGetValue(type, out var currentRegistry))
                currentRegistry.RegisterAll(registry.GetEnumerator().ToIEnumerable());
            else
                _registries[type] = registry.Clone();
        }
    }

    public IEnumerator<KeyValuePair<Type, Registry>> GetEnumerator() => _registries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
