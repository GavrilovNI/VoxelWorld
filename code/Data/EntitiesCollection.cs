using VoxelWorld.Entities;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Data;

public class EntitiesCollection : IEnumerable<Entity>
{
    public event Action<Entity, Vector3Int, Vector3Int>? EntityMovedToAnotherChunk;

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
            entity.MovedToAnotherChunk -= OnEntityMovedToAnotherChunk;
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
            SetupEntityChunk(entity);
            entity.MovedToAnotherChunk += OnEntityMovedToAnotherChunk;
        }
    }

    protected virtual void SetupEntityChunk(Entity entity)
    {
        lock(Locker)
        {
            var chunkPosition = World.GetChunkPosition(entity.Transform.Position);
            ChunkedEntities[entity] = chunkPosition;
        }
    }

    protected virtual void UpdateEntityChunk(Entity entity)
    {
        lock(Locker)
        {
            var oldChunkPosition = ChunkedEntities[entity];
            var chunkPosition = World.GetChunkPosition(entity.Transform.Position);
            ChunkedEntities[entity] = chunkPosition;
        }
    }

    protected virtual void OnEntityMovedToAnotherChunk(Entity entity, Vector3Int oldChunkPosition, Vector3Int newChunkPosition)
    {
        UpdateEntityChunk(entity);
        EntityMovedToAnotherChunk?.Invoke(entity, oldChunkPosition, newChunkPosition);
    }

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
