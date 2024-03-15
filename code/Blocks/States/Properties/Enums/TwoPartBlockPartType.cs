using VoxelWorld.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VoxelWorld.Blocks.States.Properties.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
[JsonConverter(typeof(CustomEnumJsonConverter))]
public class TwoPartBlockPartType : CustomEnum<TwoPartBlockPartType>, ICustomEnum<TwoPartBlockPartType>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly TwoPartBlockPartType First = new(0, "First");
    public static readonly TwoPartBlockPartType Second = new(1, "Second");

    public static IReadOnlyList<TwoPartBlockPartType> All { get; } = new List<TwoPartBlockPartType>() { First, Second }.AsReadOnly();

    [Obsolete("For serialization only", true)]
    public TwoPartBlockPartType()
    {
    }

    private TwoPartBlockPartType(int ordinal, string name) : base(ordinal, name)
    {
    }

    public TwoPartBlockPartType GetOpposite() => this == First ? Second : First;

    public static bool TryParse(string name, out TwoPartBlockPartType value) => TryParse(All, name, out value);

    public static explicit operator TwoPartBlockPartType(int ordinal) => All[ordinal];
    public static bool operator ==(TwoPartBlockPartType a, TwoPartBlockPartType b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(TwoPartBlockPartType a, TwoPartBlockPartType b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
