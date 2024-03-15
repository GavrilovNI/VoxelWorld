using VoxelWorld.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Blocks.States.Properties;

public class BlockProperty
{
    public readonly Id Id;
    public readonly Type PropertyType;

    internal BlockProperty(Id id, Type propertyType)
    {
        Id = id;
        PropertyType = propertyType;
    }

    public virtual IEnumerable<CustomEnum> GetAllValues() => CustomEnum.GetValues(PropertyType);

    public bool IsValidValue(CustomEnum customEnum) => customEnum.GetType() == PropertyType && GetAllValues().Contains(customEnum);

    public static bool operator ==(BlockProperty a, BlockProperty b) => a.PropertyType == b.PropertyType && a.Id == b.Id;
    public static bool operator !=(BlockProperty a, BlockProperty b) => a.PropertyType != b.PropertyType || a.Id != b.Id;

    public virtual bool Equals(BlockProperty property) => this == property;
    public override bool Equals(object? obj) => obj is BlockProperty property && Equals(property);

    public override int GetHashCode() => HashCode.Combine(Id, PropertyType);
}

public class BlockProperty<T> : BlockProperty where T : CustomEnum<T>, ICustomEnum<T>
{
    public BlockProperty(Id id) : base(id, typeof(T))
    {
    }
}
