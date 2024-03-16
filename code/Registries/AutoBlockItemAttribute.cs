using Sandbox;
using VoxelWorld.Mth;
using System;

namespace VoxelWorld.Registries;

[AttributeUsage(AttributeTargets.Property)]
public class AutoBlockItemAttribute : Attribute
{
    public readonly string ModId;
    public readonly string? BlockId;
    public readonly bool UseRawTexture = false;
    public readonly string? RawTexturePath;
    public readonly bool UseFlatModel = false;
    public readonly string? FlatModelTexturePath;
    public readonly int StackLimit;

    public AutoBlockItemAttribute(string modId, string? blockId = null, string? rawTexturePath = null, string? flatModelTexturePath = null, int stackLimit = 0)
    {
        ModId = modId;
        BlockId = blockId;
        UseRawTexture = rawTexturePath != null;
        RawTexturePath = rawTexturePath;
        UseFlatModel = flatModelTexturePath != null;
        FlatModelTexturePath = flatModelTexturePath;
        StackLimit = stackLimit <= 0 ? DefaultValues.ItemStackLimit : stackLimit;
    }

    public AutoBlockItemAttribute(string modId, string texturePath, bool useRawTexture = true, bool useFlatModel = true, string? blockId = null, int stackLimit = 0)
    {
        ModId = modId;
        BlockId = blockId;
        UseRawTexture = useRawTexture;
        RawTexturePath = useRawTexture ? texturePath : null;
        UseFlatModel = useFlatModel;
        FlatModelTexturePath = useFlatModel ? texturePath : null;
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
