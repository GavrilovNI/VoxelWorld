using System;

namespace Sandcube.Registries;

[AttributeUsage(AttributeTargets.Property)]
public class AutoBlockItemAttribute : Attribute
{
    public readonly string ModId;
    public readonly string? BlockId;

    public AutoBlockItemAttribute(string modId, string? blockId)
    {
        ModId = modId;
        BlockId = blockId;
    }

    public AutoBlockItemAttribute(string modId)
    {
        ModId = modId;
        BlockId = null;
    }
}
