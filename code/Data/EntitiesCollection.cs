using Sandcube.Entities;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Data;

public class EntitiesCollection : IEnumerable<Entity>
{
    protected readonly object Locker = new();

    protected readonly IWorldProvider World;

    protected readonly Dictionary<Guid, Entity> Entities = new();
    protected readonly MultiValueBiDictionary<Vector3Int, Entity> ChunkedEntities = new();


    public EntitiesCollection(IWorldProvider world)
    {
        World = world;
    }

    public object GetLocker() => Locker;

    public void Clear(bool destroyEntities = true)
    {
        lock(Locker)
        {
            foreach(var entity in Entities.Values.ToList())
            {
                Remove(entity);
                if(destroyEntities)
                    entity.Destroy();
            }
        }
    }

    public bool TryGet(Guid entityId, out Entity entity)
    {
        lock(Locker)
        {
            if(Entities.TryGetValue(entityId, out entity!))
            {
                if(entity.IsValid)
                    return true;
            }
        }
        entity = null!;
        return false;
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
                return RemoveInternal(entity);
        }
        return false;
    }

    public bool Remove(Entity entity)
    {
        lock(Locker)
        {
            if(Entities.TryGetValue(entity.Id, out var realEntity) && realEntity == entity)
                return RemoveInternal(realEntity);
        }
        return false;
    }

    private bool RemoveInternal(Entity entity)
    {
        lock(Locker)
        {
            entity.Moved -= OnEntityMoved;
            entity.Destroyed -= OnEntityDestroyed;
            Entities.Remove(entity.Id);
            ChunkedEntities.RemoveValue(entity);
            return true;
        }
    }

    public void Add(Entity entity)
    {
        lock(Locker)
        {
            if(Has(entity.Id))
                throw new InvalidOperationException($"{nameof(Entity)} with id {entity.Id} was already presented");

            Entities.Add(entity.Id, entity);
            UpdateEntityChunk(entity);
            entity.Moved += OnEntityMoved;
            entity.Destroyed += OnEntityDestroyed;
        }
    }

    protected virtual void UpdateEntityChunk(Entity entity)
    {
        lock(Locker)
        {
            var chunkPosition = World.GetChunkPosition(entity.Transform.Position);
            if(ChunkedEntities.TryGetKey(entity, out var oldChunkPosition))
            {
                if(chunkPosition == oldChunkPosition)
                    return;
            }

            ChunkedEntities[entity] = chunkPosition;
        }
    }

    protected virtual void OnEntityMoved(Entity entity, Vector3 oldPosition, Vector3 newPosition)
    {
        lock(Locker)
        {
            if(!Entities.TryGetValue(entity.Id, out var realEntity) || realEntity != entity)
                return;
            UpdateEntityChunk(entity);
        }
    }

    protected virtual void OnEntityDestroyed(Entity entity) => Remove(entity);

    public virtual IReadOnlySet<Entity> GetEntitiesInChunk(Vector3Int chunkPosition)
    {
        lock(Locker)
        {
            return ChunkedEntities.GetValuesOrEmpty(chunkPosition).ToHashSet();
        }
    }

    public virtual IReadOnlyMultiValueBiDictionary<Vector3Int, Entity> GetChunkedEntities()
    {
        lock(Locker)
        {
            return new MultiValueBiDictionary<Vector3Int, Entity>(ChunkedEntities);
        }
    }

    public virtual IEnumerator<Entity> GetEnumerator()
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
