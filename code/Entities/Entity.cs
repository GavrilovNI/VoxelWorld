using Sandbox;
using Sandcube.Registries;
using Sandcube.Worlds;
using System;
using System.Linq;

namespace Sandcube.Entities;

public abstract class Entity : Component
{
    public bool Initialized { get; private set; }
    public ModedId TypeId { get; private set; }
    public IWorldAccessor? World { get; private set; }

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
}
