using System.Collections.Generic;

namespace Sandcube.Mth.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public sealed class Enum<T> : CustomEnum<Enum<T>>, ICustomEnum<Enum<T>> where T : struct, Enum, IComparable
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public T Value { get; init; }

#pragma warning disable SB3000 // Hotloading not supported
    public static IReadOnlyList<Enum<T>> All { get; } = Enum.GetValues<T>().Select(v => new Enum<T>(v)).ToList().AsReadOnly();
#pragma warning restore SB3000 // Hotloading not supported

    [Obsolete("For serialization only", true)]
    public Enum()
    {
    }

    internal Enum(T value) : base(Array.IndexOf(Enum.GetValues<T>(), value), Enum.GetName(value)!)
    {
        Value = value;
    }

    public static bool TryParse(string name, out Enum<T> value) => TryParse(All, name, out value);

    public static explicit operator Enum<T>(int ordinal) => All[ordinal];
    public static explicit operator Enum<T>(T value) => All.First(v => v.Value.CompareTo(value) == 0);
    public static explicit operator T(Enum<T> property) => property.Value;
    public static bool operator ==(Enum<T> a, Enum<T> b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(Enum<T> a, Enum<T> b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
