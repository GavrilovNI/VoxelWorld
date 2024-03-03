using Sandbox;
using Sandcube.Mth;
using System;

namespace Sandcube.Registries;

[AttributeUsage(AttributeTargets.Property)]
public class AutoBlockItemAttribute : Attribute
{
    public readonly string ModId;
    public readonly string? BlockId;
    public readonly bool UseRawTexture = false;
    public readonly string? RawTexturePath;
    public readonly int StackLimit;

    public AutoBlockItemAttribute(string modId, string? blockId = null, string? rawTexturePath = null, int stackLimit = 0)
    {
        ModId = modId;
        BlockId = blockId;
        UseRawTexture = rawTexturePath != null;
        RawTexturePath = rawTexturePath;
        StackLimit = stackLimit <= 0 ? DefaultValues.ItemStackLimit : stackLimit;
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
