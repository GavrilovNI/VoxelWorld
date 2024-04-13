using VoxelWorld.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VoxelWorld.Blocks.States.Properties.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
[JsonConverter(typeof(CustomEnumJsonConverter))]
public class SlabType : CustomEnum<SlabType>, ICustomEnum<SlabType>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly SlabType Bottom = new(0, "Bottom");
    public static readonly SlabType Top = new(1, "Top");
    public static readonly SlabType Double = new(2, "Double");

    public static IReadOnlyList<SlabType> All { get; } = new List<SlabType>() { Bottom, Top, Double }.AsReadOnly();

    [Obsolete("For serialization only", true)]
    public SlabType()
    {
    }

    private SlabType(int ordinal, string name) : base(ordinal, name)
    {
    }

    public SlabType GetOpposite()
    {
        if(this == Bottom)
            return Top;
        if(this == Top)
            return Bottom;
        return Double;
    }

    public SlabType Combine(SlabType other)
    {
        if(this == other)
            return this;
        return SlabType.Double;
    }

    public static bool TryParse(string name, out SlabType value) => TryParse(All, name, out value);

    public static explicit operator SlabType(int ordinal) => All[ordinal];
    public static bool operator ==(SlabType a, SlabType b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(SlabType a, SlabType b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
