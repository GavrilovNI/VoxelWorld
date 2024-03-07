using Sandbox;
using Sandcube.Entities.Types;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values.Sandboxed;
using Sandcube.Registries;
using Sandcube.Worlds;
using System;
using System.IO;
using System.Linq;

namespace Sandcube.Entities;

public abstract class Entity : Component
{
    public bool Initialized { get; private set; }
    public ModedId TypeId { get; private set; }
    public IWorldAccessor? World { get; private set; }

    public Guid Id => GameObject.Id;

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

        Tags.Add("entity");

        TypeId = typeId;
        ChangeWorld(world);
    }

    public bool ChangeWorld(IWorldAccessor? newWorld)
    {
        if(object.ReferenceEquals(World, newWorld))
            return false;

        var oldWorld = World;
        World = newWorld;
        oldWorld?.RemoveEntity(this);
        World?.AddEntity(this);
        return true;
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
    }

    protected virtual void OnDestroyInternal()
    {

    }

    protected virtual BinaryTag WriteAdditional() => new CompoundTag();
    protected virtual void ReadAdditional(BinaryTag tag) { }

    protected virtual void WriteAdditional(BinaryWriter writer) { }
    protected virtual void ReadAdditional(BinaryReader reader) { }

    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("type_id", TypeId);
        var transform = World!.GameObject.Transform.World.ToLocal(Transform.World);
        tag.Set("transform", transform);

        var additionalData = WriteAdditional();
        if(!additionalData.IsDataEmpty)
            tag.Set("data", additionalData);
        return tag;
    }

    public static Entity Read(BinaryTag tag, IWorldAccessor world, bool enable = true)
    {
        CompoundTag compoundTag = (CompoundTag)tag;
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

    public void Write(BinaryWriter writer)
    {
        writer.Write<ModedId>(TypeId);
        writer.Write(Transform.Local);
        WriteAdditional(writer);
    }

    public static Entity Read(BinaryReader reader, IWorldAccessor? world, bool enable = true)
    {
        var typeId = ModedId.Read(reader);

        var entityType = SandcubeGame.Instance!.Registries.GetRegistry<EntityType>().Get(typeId);
        EntitySpawnConfig spawnConfig = new(world, false);
        var entity = entityType.CreateEntity(spawnConfig);

        entity.Transform.Local = reader.ReadTransform();
        entity.ReadAdditional(reader);

        entity.Enabled = enable;
        return entity;
    }
}
