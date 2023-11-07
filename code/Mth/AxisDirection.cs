using Sandcube.Components;
using System.Collections.Generic;

namespace Sandcube.Mth;

public sealed record class AxisDirection
{
    [CustomEnumValue]
    public static readonly AxisDirection Positive = new("positive", 1);
    [CustomEnumValue]
    public static readonly AxisDirection Negative = new("negative", -1);

    public static readonly IReadOnlyList<AxisDirection> All = new List<AxisDirection>() { Positive, Negative }.AsReadOnly();


    public string Name { get; init; }
    public int Normal { get; init; }

    [Obsolete("For serialization only", true)]
    public AxisDirection()
    {
    }

    private AxisDirection(string name, int normal)
    {
        Name = name;
        Normal = normal;
    }

    public Direction With(Axis axis) => Direction.Of(axis, this);

    public AxisDirection GetOpposite() => this == Positive ? Negative : Positive;
}
