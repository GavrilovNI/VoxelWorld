using Sandbox;
using Sandcube.Entities.Types;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values.Sandboxed;
using Sandcube.Mth;
using Sandcube.Registries;
using Sandcube.Worlds;
using System;
using System.Linq;

namespace Sandcube.Entities;

public abstract class Entity : Component
{
    public event Action<Entity, IWorldAccessor?, IWorldAccessor?>? ChangedWorld;
    public event Action<Entity, Vector3Int, Vector3Int>? MovedToAnotherChunk;
    public event Action<Entity, Transform, Transform>? TransformChanged;
    public event Action<Entity>? Destroyed;

    public bool Initialized { get; private set; }
    public ModedId TypeId { get; private set; }
    public IWorldAccessor? World { get; private set; }
    public Vector3Int ChunkPosition { get; private set; }

    private Transform _oldTransform;

    public new Guid Id => GameObject.Id;

    public new bool Enabled
    {
        get => GameObject.Enabled;
        set => GameObject.Enabled = value;
    }

    public void Initialize(ModedId typeId, IWorldAccessor? world = null)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Entity)} {this} was already initialized");
        Initialized = true;

        TypeId = typeId;
        ChangeWorld(world);
    }

    public bool ChangeWorld(IWorldAccessor? newWorld)
    {
        if(!IsValid && newWorld is not null)
            throw new InvalidOperationException($"{this} is not valid, can't change world (remove is possible)");

        if(object.ReferenceEquals(World, newWorld))
            return false;

        var oldWorld = World;
        World = newWorld;
        oldWorld?.RemoveEntity(this);
        World?.AddEntity(this);
        ChunkPosition = World?.GetChunkPosition(Transform.Position) ?? Vector3Int.Zero;
        OnChangedWorld(oldWorld, World);
        ChangedWorld?.Invoke(this, oldWorld, World);
        return true;
    }

    protected virtual void OnChangedWorld(IWorldAccessor? oldWorld, IWorldAccessor? newWorld)
    {

    }

    protected sealed override void OnAwake()
    {
        Tags.Add("entity");

        _oldTransform = Transform.World;
        Transform.OnTransformChanged = () =>
        {
            if(World is not null)
            {
                var newChunkPosition = World?.GetChunkPosition(Transform.Position) ?? Vector3Int.Zero;
                if(newChunkPosition != ChunkPosition)
                {
                    var oldChunkPosition = ChunkPosition;
                    ChunkPosition = newChunkPosition;
                    OnMovedToAnotherChunk(oldChunkPosition, newChunkPosition);
                    MovedToAnotherChunk?.Invoke(this, oldChunkPosition, newChunkPosition);
                }
            }
            var oldTransform = _oldTransform;
            _oldTransform = Transform.World;
            OnTransformChanged(oldTransform, Transform.World);
            TransformChanged?.Invoke(this, oldTransform, Transform.World);
        };
        OnAwakeInternal();
    }

    protected virtual void OnAwakeInternal()
    {

    }

    protected virtual void OnTransformChanged(Transform oldTransform, Transform newTransform)
    {

    }

    protected sealed override void OnStart()
    {
        if(!Initialized)
        {
            Log.Warning($"{nameof(Entity)} {this} was not initialized before OnStart, destroying...");
            GameObject.Destroy();
            return;
        }

        if(GameObject.Components.GetAll<Entity>(FindMode.EverythingInSelf).Count() > 1)
        {
            Log.Warning($"{nameof(GameObject)} has to many {nameof(Entity)} {this}, destroying...");
            Destroy();
            return;
        }

        OnStartInternal();
    }

    protected virtual void OnStartInternal()
    {

    }

    public new void Destroy()
    {
        Enabled = false;
        GameObject.Destroy();
    }

    protected sealed override void OnDestroy()
    {
        if(!Initialized)
            return;

        if(World is not null)
            ChangeWorld(null);

        OnDestroyInternal();
        Destroyed?.Invoke(this);
    }

    protected virtual void OnDestroyInternal()
    {

    }

    protected virtual void OnMovedToAnotherChunk(Vector3Int oldChunkPosition, Vector3Int newChunkPosition)
    {

    }

    protected virtual BinaryTag WriteAdditional() => new CompoundTag();
    protected virtual void ReadAdditional(BinaryTag tag) { }

    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("type_id", TypeId);
        tag.Set("world_id", World!.Id);
        var transform = World!.GameObject.Transform.World.ToLocal(Transform.World);
        tag.Set("transform", transform);

        var additionalData = WriteAdditional();
        if(!additionalData.IsDataEmpty)
            tag.Set("data", additionalData);
        return tag;
    }

    public static Entity Read(BinaryTag tag, IWorldAccessor world, bool enable = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        var typeId = ModedId.Read(compoundTag.GetTag("type_id"));

        var entityType = SandcubeGame.Instance!.Registries.GetRegistry<EntityType>().Get(typeId);
        EntitySpawnConfig spawnConfig = new(world, false);
        var entity = entityType.CreateEntity(spawnConfig);

        var transform = compoundTag.Get<Transform>("transform");
        entity.Transform.World = world.GameObject.Transform.Local.ToWorld(transform);

        entity.ReadAdditional(compoundTag.GetTag("data"));

        entity.Enabled = enable;
        return entity;
    }

    public static bool TryReadWithWorld(BinaryTag tag, out Entity entity, bool enable = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        var typeId = ModedId.Read(compoundTag.GetTag("type_id"));
        var worldId = ModedId.Read(compoundTag.GetTag("world_id"));
        if(!SandcubeGame.Instance!.Worlds.TryGetWorld(worldId, out World world))
        {
            entity = null!;
            return false;
        }

        var entityType = SandcubeGame.Instance!.Registries.GetRegistry<EntityType>().Get(typeId);
        EntitySpawnConfig spawnConfig = new(world, false);
        entity = entityType.CreateEntity(spawnConfig);

        var transform = compoundTag.Get<Transform>("transform");
        entity.Transform.World = world.GameObject.Transform.Local.ToWorld(transform);

        entity.ReadAdditional(compoundTag.GetTag("data"));

        entity.Enabled = enable;
        return true;
    }
}
