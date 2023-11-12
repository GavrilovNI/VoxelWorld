using System.Collections.Generic;

namespace Sandcube.Mth;

public class CustomEnum
{
    public int Ordinal { get; init; }
    public string Name { get; init; }

    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {
        Ordinal = -1;
        Name = string.Empty;
    }

    public CustomEnum(int ordinal, string name)
    {
        Ordinal = ordinal;
        Name = name;
    }
}

public class CustomEnum<T> : CustomEnum where T : CustomEnum<T>, ICustomEnum<T>
{
    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {
    }

    public CustomEnum(int ordinal, string name) : base(ordinal, name)
    {
    }

    protected static bool TryParse(IReadOnlyList<T> all, string name, out T value)
    {
        foreach (var item in all)
        {
            if(item.Name == name)
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
