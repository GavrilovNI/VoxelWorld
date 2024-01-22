using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Blocks.States.Properties;

public class BlockProperty
{
    public readonly string Name;
    public readonly Type PropertyType;
    public readonly CustomEnum DefaultValue;

    public BlockProperty(string name, Type propertyType, CustomEnum defaultValue)
    {
        Name = name;
        PropertyType = propertyType;
        DefaultValue = defaultValue;

        if(!IsValidValue(DefaultValue))
            throw new ArgumentException($"{defaultValue} is not valid for this {nameof(BlockProperty)}", nameof(defaultValue));
    }

    public virtual IEnumerable<CustomEnum> GetAllValues() => DefaultValue.GetAll();

    public bool IsValidValue(CustomEnum customEnum) => customEnum.GetType() == PropertyType && GetAllValues().Contains(customEnum);

    public static bool operator ==(BlockProperty a, BlockProperty b) => a.PropertyType == b.PropertyType && a.Name == b.Name;
    public static bool operator !=(BlockProperty a, BlockProperty b) => a.PropertyType != b.PropertyType || a.Name != b.Name;

    public virtual bool Equals(BlockProperty property) => this == property;
    public override bool Equals(object? obj) => obj is BlockProperty property && Equals(property);

    public override int GetHashCode() => HashCode.Combine(Name, PropertyType);
}

public class BlockProperty<T> : BlockProperty where T : CustomEnum<T>, ICustomEnum<T>
{
    public BlockProperty(string name, T defaultValue) : base(name, typeof(T), defaultValue)
    {
    }
}
