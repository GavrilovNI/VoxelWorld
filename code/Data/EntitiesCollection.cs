using Sandcube.Entities;
using Sandcube.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Data;

public class EntitiesCollection : IEnumerable<Entity>
{
    protected readonly object Locker = new();

    protected readonly Dictionary<Guid, Entity> Entities = new();

    public object GetLocker() => Locker;


    public void Clear()
    {
        lock(Locker)
        {
            Entities.Clear();
        }
    }

    public bool TryGet(Guid entityId, out Entity entity)
    {
        lock(Locker)
        {
            return Entities.TryGetValue(entityId, out entity!) && entity.IsValid;
        }
    }

    public bool Has(Guid entityId) => TryGet(entityId, out _);

    public Entity? Get(Guid entityId)
    {
        if(TryGet(entityId, out var entity))
            return entity;
        return null;
    }

    public bool Remove(Guid entityId)
    {
        lock(Locker)
        {
            if(Entities.TryGetValue(entityId, out var entity))
            {
                Entities.Remove(entityId);
                return entity.IsValid;
            }
        }
        return false;
    }

    public bool Remove(Entity entity)
    {
        lock(Locker)
        {
            if(Entities.TryGetValue(entity.Id, out var realEntity) && realEntity == entity)
            {
                Entities.Remove(realEntity.Id);
                return realEntity.IsValid;
            }
        }
        return false;
    }

    public void Add(Entity entity)
    {
        lock(Locker)
        {
            if(Has(entity.Id))
                throw new InvalidOperationException($"{nameof(Entity)} with id {entity.Id} was already presented");

            Entities.Add(entity.Id, entity);
        }
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        lock(Locker)
        {
            foreach(var (_, entity) in Entities)
            {
                if(entity.IsValid)
                    yield return entity;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
