using Sandbox;
using System;

namespace Sandcube.Registries;

public class AutoPrefabEntityTypeAttribute : Attribute
{
    public readonly string ModId;
    public readonly string? PrefabPropertyName;
    public readonly string? EntityId;

    public AutoPrefabEntityTypeAttribute(string modId, string? prefabPropertyName = null, string? entityId = null)
    {
        ModId = modId;
        PrefabPropertyName = prefabPropertyName;
        EntityId = entityId;
    }

    public string GetPrefabPropertyName(PropertyDescription property) => PrefabPropertyName ?? $"{property.Name}Prefab";

    public bool TryGetModedId(PropertyDescription property, out ModedId modedId)
    {
        var blockIdString = EntityId ?? property.Name;
        if(!Id.TryFromCamelCase(blockIdString, out Id entityId))
        {
            modedId = default;
            return false;
        }

        if(!Id.TryFromCamelCase(ModId, out Id modId))
        {
            modedId = default;
            return false;
        }

        modedId = new(modId, entityId);
        return true;
    }
}
