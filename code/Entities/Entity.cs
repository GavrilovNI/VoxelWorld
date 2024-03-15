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
using System.Threading.Tasks;

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
    private bool tranformChanged = false;

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

    protected sealed override void OnEnabled() => OnEnabledChild();
    protected virtual void OnEnabledChild() { }

    protected sealed override void OnAwake()
    {
        Tags.Add("entity");

        _oldTransform = Transform.World;
        Transform.OnTransformChanged = () =>
        {
            tranformChanged = true;
        };
        OnAwakeChild();
    }
    protected virtual void OnAwakeChild() { }

    private void HandleTransformChanging()
    {
        if(!tranformChanged)
            return;

        tranformChanged = false;
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
    }

    protected virtual void OnTransformChanged(Transform oldTransform, Transform newTransform) { }

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

        OnStartChild();
    }
    protected virtual void OnStartChild() { }

    protected sealed override void OnUpdate()
    {
        HandleTransformChanging();
        OnUpdateChild();
    }
    protected virtual void OnUpdateChild() { }

    protected sealed override void OnFixedUpdate()
    {
        HandleTransformChanging();
        OnFixedUpdateChild();
    }
    protected virtual void OnFixedUpdateChild() { }

    protected sealed override void OnDisabled() => OnDisabledChild();
    protected virtual void OnDisabledChild() { }

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

        OnDestroyChild();
        Destroyed?.Invoke(this);
    }

    protected virtual void OnDestroyChild() { }


    protected sealed override void OnDirty() => OnDirtyChild();
    protected virtual void OnDirtyChild() { }
    protected sealed override Task OnLoad() => OnLoadChild();
    protected virtual Task OnLoadChild() => Task.CompletedTask;
    protected sealed override void DrawGizmos() => DrawGizmosChild();
    protected virtual void DrawGizmosChild() { }
    protected sealed override void OnParentChanged(GameObject oldParent, GameObject newParent) => OnParentChangedChild(oldParent, newParent);
    protected virtual void OnParentChangedChild(GameObject oldParent, GameObject newParent) { }
    protected sealed override void OnPreRender() => OnPreRenderChild();
    protected virtual void OnPreRenderChild() { }
    protected sealed override void OnTagsChanged() => OnTagsChangedChild();
    protected virtual void OnTagsChangedChild() { }
    protected sealed override void OnValidate() => OnValidateChild();
    protected virtual void OnValidateChild() { }


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
