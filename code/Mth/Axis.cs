using Sandcube.Components;
using System.Collections.Generic;

namespace Sandcube.Mth;

public sealed record class Axis
{
    [CustomEnumValue]
    public static readonly Axis X = new("x", new Vector3Int(1, 0, 0));
    [CustomEnumValue]
    public static readonly Axis Y = new("y", new Vector3Int(0, 1, 0));
    [CustomEnumValue]
    public static readonly Axis Z = new("z", new Vector3Int(0, 0, 1));

    public static readonly IReadOnlyList<Axis> All = new List<Axis>() { X, Y, Z }.AsReadOnly();


    public string Name { get; init; }
    public Vector3Int PositiveNormal { get; init; }

    [Obsolete("For serialization only", true)]
    public Axis()
    {
    }

    private Axis(string name, Vector3Int positiveNormal)
    {
        Name = name;
        PositiveNormal = positiveNormal;
    }

    public Direction With(AxisDirection axisDirection) => Direction.Of(this, axisDirection);
}
