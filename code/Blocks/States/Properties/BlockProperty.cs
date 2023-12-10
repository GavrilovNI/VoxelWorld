using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;

namespace Sandcube.Blocks.States.Properties;

public abstract class BlockProperty
{
    public readonly string Name;
    public readonly Type PropertyType;
    public readonly CustomEnum DefaultValue;

    public BlockProperty(string name, Type propertyType, CustomEnum defaultValue)
    {
        if(defaultValue.GetType() != propertyType)
            throw new ArgumentException($"{nameof(defaultValue)} should be type of ${propertyType}");

        Name = name;
        PropertyType = propertyType;
        DefaultValue = defaultValue;
    }

    public abstract IEnumerable<CustomEnum> GetAllValues();

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

    public override IEnumerable<CustomEnum> GetAllValues() => DefaultValue.GetAll();

}
