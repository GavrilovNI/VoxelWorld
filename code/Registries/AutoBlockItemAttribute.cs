using System;

namespace Sandcube.Registries;

[AttributeUsage(AttributeTargets.Property)]
public class AutoBlockItemAttribute : Attribute
{
    public string ModId;
    public string? BlockId;

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
