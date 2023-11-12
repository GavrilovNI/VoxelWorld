using System.Collections.Generic;

namespace Sandcube.Mth;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public sealed class AxisDirection : CustomEnum<AxisDirection>, ICustomEnum<AxisDirection>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly AxisDirection Positive = new(0, "Positive", 1);
    public static readonly AxisDirection Negative = new(1, "Negative", -1);

    public static IReadOnlyList<AxisDirection> All { get; private set; } = new List<AxisDirection>() { Positive, Negative }.AsReadOnly();
    public static readonly IReadOnlySet<AxisDirection> AllSet = All.ToHashSet();


    public int Normal { get; init; }

    [Obsolete("For serialization only", true)]
    public AxisDirection()
    {
    }

    private AxisDirection(int ordinal, string name, int normal) : base(ordinal, name)
    {
        Normal = normal;
    }
    public static bool TryParse(string name, out AxisDirection value) => TryParse(All, name, out value);


    public Direction With(Axis axis) => Direction.Of(axis, this);

    public AxisDirection GetOpposite() => this == Positive ? Negative : Positive;

    public static explicit operator AxisDirection(int ordinal) => All[ordinal];

    public static bool operator ==(AxisDirection a, AxisDirection b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(AxisDirection a, AxisDirection b) => a.Ordinal != b.Ordinal;
}
