using Sandbox;
using Sandcube.Registries;

namespace Sandcube.Entities.Types;

public class PrefabEntityType<T> : EntityType<T> where T : Entity
{
    public PrefabScene Prefab { get; }

    public PrefabEntityType(in ModedId id, PrefabScene prefab) : base(id)
    {
        Prefab = prefab;
    }

    public override T CreateEntityT(in EntitySpawnConfig spawnConfig)
    {
        GameObject gameObject = Prefab.Clone(spawnConfig.ToCloneConfig());
        T entity = gameObject.Components.Get<T>(FindMode.EverythingInSelf);
        entity.Initialize(Id, spawnConfig.World);
        return entity;
    }
}
