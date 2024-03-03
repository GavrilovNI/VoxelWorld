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

    public bool TryAddWorld(World world)
    {
        if(!world.Initialized)
            throw new InvalidOperationException($"{nameof(World)} {world} wasn't initialized");

        if(HasWorld(world.Id))
            return false;

        _worlds[world.Id] = world;
        return true;
    }

    public void AddWorld(World world)
    {
        if(!TryAddWorld(world))
            throw new ArgumentException($"{world.Id} was already presented");
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

    public IEnumerator<World> GetEnumerator()
    {
        foreach(var (_, world) in _worlds)
        {
            if(!world.IsValid())
                continue;
            yield return world;
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
