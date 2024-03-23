using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace VoxelWorld.Mth.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
[JsonConverter(typeof(CustomEnumJsonConverter))]
public sealed class Axis : CustomEnum<Axis>, ICustomEnum<Axis>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly Axis X = new(0, "X", new Vector3IntB(1, 0, 0));
    public static readonly Axis Y = new(1, "Y", new Vector3IntB(0, 1, 0));
    public static readonly Axis Z = new(2, "Z", new Vector3IntB(0, 0, 1));

    public static IReadOnlyList<Axis> All { get; } = new List<Axis>() { X, Y, Z }.AsReadOnly();
    public static readonly IReadOnlySet<Axis> AllSet = All.ToHashSet();
    public static IReadOnlyList<Axis> XY { get; } = new List<Axis>() { X, Y }.AsReadOnly();
    public static readonly IReadOnlySet<Axis> XYSet = XY.ToHashSet();

    public Vector3IntB PositiveNormal { get; init; }

    [Obsolete("For serialization only", true)]
    public Axis()
    {
    }

    private Axis(int ordinal, string name, Vector3IntB positiveNormal) : base(ordinal, name)
    {
        PositiveNormal = positiveNormal;
    }
    public static bool TryParse(string name, out Axis value) => TryParse(All, name, out value);

    public Direction With(AxisDirection axisDirection) => Direction.Of(this, axisDirection);

    public static explicit operator Axis(int ordinal) => All[ordinal];

    public static bool operator ==(Axis a, Axis b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(Axis a, Axis b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
