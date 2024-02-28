using Microsoft.Win32;
using Sandbox;
using Sandcube.Entities;
using Sandcube.Entities.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Registries;

public class ModEntities : Component
{
    public virtual Task Register(RegistriesContainer registries) => Register(registries.GetOrAddRegistry<EntityType>());
    public virtual Task Register(Registry<EntityType> registry)
    {
        AutoCreatePrefabEntityTypes();
        return ModRegisterables<EntityType>.RegisterFrom(this, registry);
    }

    protected virtual void AutoCreatePrefabEntityTypes()
    {
        var thisType = TypeLibrary.GetType(GetType());
        var entityTypes = thisType.Properties
            .Where(p => p.PropertyType.IsAssignableTo(typeof(EntityType)) && p.HasAttribute<AutoPrefabEntityTypeAttribute>() && p.GetValue(this) is null);

        foreach(var property in entityTypes)
        {
            if(!property.CanWrite)
            {
                Log.Warning($"Couldn't create entity {thisType.FullName}.{property.Name} as set method is not available.");
                continue;
            }

            var autoAttribute = property.GetCustomAttribute<AutoPrefabEntityTypeAttribute>();
            if(!autoAttribute.TryGetModedId(property, out ModedId entityId))
            {
                Log.Warning($"Couldn't create {nameof(ModedId)} to create entity {thisType.FullName}.{property.Name}");
                continue;
            }
            string prefabPropertyName = autoAttribute.GetPrefabPropertyName(property);
            var prefab = thisType.GetProperty(prefabPropertyName)?.GetValue(this) as PrefabScene;
            if(prefab is null)
            {
                Log.Warning($"Couldn't find prefab property {prefabPropertyName}(or property was null) to create entity {thisType.FullName}.{property.Name}");
                continue;
            }

            var entityType = new PrefabEntityType<Entity>(entityId, prefab);
            property.SetValue(this, entityType);

            if(property.GetValue(this) is null)
            {
                Log.Warning($"Couldn't set {nameof(PrefabEntityType<Entity>)} to {thisType.FullName}.{property.Name}");
                continue;
            }
        }
    }
}
