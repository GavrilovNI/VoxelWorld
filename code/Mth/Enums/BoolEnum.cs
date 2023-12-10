using System;
using System.Collections.Generic;

namespace Sandcube.Mth.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public sealed class BoolEnum : CustomEnum<BoolEnum>, ICustomEnum<BoolEnum>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly BoolEnum False = new(0, "False", false);
    public static readonly BoolEnum True = new(1, "True", true);

    public bool Value { get; init; }

    public static IReadOnlyList<BoolEnum> All { get; } = new List<BoolEnum>() { False, True }.AsReadOnly();

    [Obsolete("For serialization only", true)]
    public BoolEnum()
    {
    }

    private BoolEnum(int ordinal, string name, bool value) : base(ordinal, name)
    {
        Value = value;
    }

    public static bool TryParse(string name, out BoolEnum value) => TryParse(All, name, out value);

    public static explicit operator BoolEnum(int ordinal) => All[ordinal];
    public static explicit operator BoolEnum(bool value) => value ? True : False;
    public static explicit operator bool(BoolEnum property) => property.Value;
    public static bool operator ==(BoolEnum a, BoolEnum b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(BoolEnum a, BoolEnum b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
