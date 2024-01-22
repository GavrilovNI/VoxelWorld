using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Mth.Enums;

public abstract class CustomEnum
{
    public int Ordinal { get; init; }
    public string Name { get; init; }

    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {
        Ordinal = -1;
        Name = string.Empty;
    }

    protected CustomEnum(int ordinal, string name)
    {
        Ordinal = ordinal;
        Name = name;
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public static IEnumerable<CustomEnum> GetValues(Type t)
    {
        if(!t.IsAssignableTo(typeof(CustomEnum)))
            throw new ArgumentException($"{t} is not assignable to {nameof(CustomEnum)}");
        return TypeLibrary.GetType(t).Create<CustomEnum>().GetAll();
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public abstract IEnumerable<CustomEnum> GetAll();
}

public abstract class CustomEnum<T> : CustomEnum where T : CustomEnum<T>, ICustomEnum<T>
{
    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {

    }

    protected CustomEnum(int ordinal, string name) : base(ordinal, name)
    {
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public static IEnumerable<T> GetValues() => CustomEnum.GetValues(typeof(T)).Cast<T>();

    protected static bool TryParse(IReadOnlyList<T> all, string name, out T value)
    {
        foreach(var item in all)
        {
            if(item.Name.ToLower() == name.ToLower())
            {
                value = item;
                return true;
            }
        }

        value = all[0];
        return false;
    }

    public static explicit operator int(CustomEnum<T> enumValue) => enumValue.Ordinal;

    public bool Equals(T enumValue) => Ordinal == enumValue.Ordinal;
    public sealed override bool Equals(object? obj) => obj is T enumValue && Equals(enumValue);
    public sealed override int GetHashCode() => Ordinal;

    public override string ToString() => Name;
}
