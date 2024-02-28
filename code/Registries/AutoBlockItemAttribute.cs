using Sandbox;
using System;

namespace Sandcube.Registries;

[AttributeUsage(AttributeTargets.Property)]
public class AutoBlockItemAttribute : Attribute
{
    public readonly string ModId;
    public readonly string? BlockId;
    public readonly bool UseRawTexture = false;
    public readonly string? RawTexturePath;

    public AutoBlockItemAttribute(string modId, string? blockId = null, string? rawTexturePath = null)
    {
        ModId = modId;
        BlockId = blockId;
        UseRawTexture = rawTexturePath != null;
        RawTexturePath = rawTexturePath;
    }

    public bool TryGetModedId(PropertyDescription property, out ModedId modedId)
    {
        var blockIdString = BlockId ?? property.Name;
        if(!Id.TryFromCamelCase(blockIdString, out Id blockId))
        {
            modedId = default;
            return false;
        }

        if(!Id.TryFromCamelCase(ModId, out Id modId))
        {
            modedId = default;
            return false;
        }

        modedId = new(modId, blockId);
        return true;
    }
}
