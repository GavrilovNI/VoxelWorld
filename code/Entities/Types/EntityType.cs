using Sandbox;
using Sandcube.Registries;

namespace Sandcube.Entities.Types;

public abstract class EntityType : IRegisterable
{
    public ModedId Id { get; }

    protected internal EntityType(in ModedId id)
    {
        Id = id;
    }

    public virtual void OnRegistered() { }

    public abstract Entity CreateEntity(in EntitySpawnConfig spawnConfig);
}

public abstract class EntityType<T> : EntityType where T : Entity
{
    public EntityType(in ModedId id) : base(id)
    {
    }

    public sealed override Entity CreateEntity(in EntitySpawnConfig spawnConfig) => CreateEntityT(spawnConfig);
    public abstract T CreateEntityT(in EntitySpawnConfig spawnConfig);
}
