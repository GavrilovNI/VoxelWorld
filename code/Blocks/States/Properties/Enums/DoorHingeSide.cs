using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sandcube.Blocks.States.Properties.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
[JsonConverter(typeof(CustomEnumJsonConverter))]
public class DoorHingeSide : CustomEnum<DoorHingeSide>, ICustomEnum<DoorHingeSide>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly DoorHingeSide Left = new(0, "Left");
    public static readonly DoorHingeSide Right = new(1, "Right");

    public static IReadOnlyList<DoorHingeSide> All { get; } = new List<DoorHingeSide>() { Left, Right }.AsReadOnly();

    [Obsolete("For serialization only", true)]
    public DoorHingeSide()
    {
    }

    private DoorHingeSide(int ordinal, string name) : base(ordinal, name)
    {
    }

    public DoorHingeSide GetOpposite() => this == Left ? Right : Left;

    public static bool TryParse(string name, out DoorHingeSide value) => TryParse(All, name, out value);

    public static explicit operator DoorHingeSide(int ordinal) => All[ordinal];
    public static bool operator ==(DoorHingeSide a, DoorHingeSide b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(DoorHingeSide a, DoorHingeSide b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
