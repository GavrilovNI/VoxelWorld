using Sandbox;
using Sandcube.Registries;
using Sandcube.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Data;

public class WorldsContainer : IReadOnlyWorldsContainer
{
    private readonly Dictionary<ModedId, World> _worlds = new();

    public int Count => _worlds.Count;

    public bool TryAddWorld(ModedId id, World world)
    {
        if(HasWorld(id))
            return false;

        _worlds[id] = world;
        return true;
    }

    public void AddWorld(ModedId id, World world)
    {
        if(!TryAddWorld(id, world))
            throw new ArgumentException($"{id} was already presented");
    }

    public bool HasWorld(ModedId id) => TryGetValidWorldOrRemove(id, out _);

    public bool TryGetWorld(ModedId id, out World world) => TryGetValidWorldOrRemove(id, out world);

    public bool RemoveWorld(ModedId id) => _worlds.Remove(id);

    public World GetWorld(ModedId id)
    {
        if(!TryGetValidWorldOrRemove(id, out var world))
            throw new KeyNotFoundException($"{id} wasn't found");
        return world;
    }

    public IEnumerator<KeyValuePair<ModedId, World>> GetEnumerator()
    {
        foreach(var pair in _worlds)
        {
            if(!pair.Value.IsValid())
                continue;
            yield return pair;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected bool TryGetValidWorldOrRemove(ModedId id, out World world)
    {
        if(_worlds.TryGetValue(id, out world!))
        {
            if(!world.IsValid())
            {
                _worlds.Remove(id);
                return false;
            }
            return true;
        }
        return false;
    }
}
