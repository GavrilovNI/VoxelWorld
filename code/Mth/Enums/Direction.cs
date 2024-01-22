using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Mth.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public sealed class Direction : CustomEnum<Direction>, ICustomEnum<Direction>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly Direction Forward = new(0, "Forward", Axis.X, AxisDirection.Positive);
    public static readonly Direction Backward = new(1, "Backward", Axis.X, AxisDirection.Negative);
    public static readonly Direction Left = new(2, "Left", Axis.Y, AxisDirection.Positive);
    public static readonly Direction Right = new(3, "Right", Axis.Y, AxisDirection.Negative);
    public static readonly Direction Up = new(4, "Up", Axis.Z, AxisDirection.Positive);
    public static readonly Direction Down = new(5, "Down", Axis.Z, AxisDirection.Negative);

    public static IReadOnlyList<Direction> All { get; } = new List<Direction>() { Forward, Backward, Left, Right, Up, Down }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Horizontal = new List<Direction>() { Forward, Backward, Left, Right }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Vertical = new List<Direction>() { Up, Down }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Positive = new List<Direction>() { Forward, Left, Up }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Negative = new List<Direction>() { Backward, Right, Down }.AsReadOnly();

    public static readonly IReadOnlySet<Direction> AllSet = All.ToHashSet();
    public static readonly IReadOnlySet<Direction> HorizontalSet = Horizontal.ToHashSet();
    public static readonly IReadOnlySet<Direction> VerticalSet = Vertical.ToHashSet();
    public static readonly IReadOnlySet<Direction> PositiveSet = Positive.ToHashSet();
    public static readonly IReadOnlySet<Direction> NegativeSet = Negative.ToHashSet();


    public Axis Axis { get; init; }
    public AxisDirection AxisDirection { get; init; }
    public Vector3Int Normal { get; init; }

    [Obsolete("For serialization only", true)]
    public Direction()
    {
        Axis = Axis.X;
        AxisDirection = AxisDirection.Positive;
        Normal = Vector3Int.Zero;
    }

    private Direction(int ordinal, string name, Axis axis, AxisDirection axisDirection) : base(ordinal, name)
    {
        Axis = axis;
        AxisDirection = axisDirection;
        Normal = Axis.PositiveNormal * AxisDirection.Normal;
    }
    public static bool TryParse(string name, out Direction value) => TryParse(All, name, out value);

    public static Direction Of(Axis axis, AxisDirection axisDirection)
    {
        bool isPositive = axisDirection == AxisDirection.Positive;

        if(axis == Axis.X)
            return isPositive ? Forward : Backward;
        if(axis == Axis.Y)
            return isPositive ? Left : Right;
        return isPositive ? Up : Down;
    }

    public static Direction ClosestTo(Vector3 direction, Direction directionIfZero)
    {
        Axis maxAxis = directionIfZero.Axis;
        float maxValueAbs = 0;
        float maxValue = directionIfZero.AxisDirection.Normal;

        direction.EachAxis((a, v) =>
        {
            var valueAbs = MathF.Abs(v);
            if(valueAbs > maxValueAbs)
            {
                maxValueAbs = valueAbs;
                maxValue = v;
                maxAxis = a;
            }
        });

        return Of(maxAxis, maxValue > 0 ? AxisDirection.Positive : AxisDirection.Negative);
    }
    public static Direction ClosestTo(Vector3 direction) => ClosestTo(direction, Up);

    public Direction GetOpposite() => Of(Axis, AxisDirection.GetOpposite());

    public Direction Rotate(RightAngle rotation90, Direction lookDirection) // TODO: optimize
    {
        if(this.Axis == lookDirection.Axis)
            return this;

        var rotation = Rotation.FromAxis(lookDirection.Axis.PositiveNormal, rotation90.Angle * lookDirection.AxisDirection.Normal);
        return Direction.ClosestTo(Normal * rotation);
    }

    public static explicit operator Direction(int ordinal) => All[ordinal];
    public static implicit operator Vector3Int(Direction direction) => direction.Normal;

    public static Direction operator -(Direction direction) => direction.GetOpposite();
    public static Vector3Int operator *(Direction direction, int value) => direction.Normal * value;
    public static Vector3Int operator *(int value, Direction direction) => direction.Normal * value;
    public static Vector3 operator *(Direction direction, float value) => direction.Normal * value;
    public static Vector3 operator *(float value, Direction direction) => direction.Normal * value;

    public static bool operator ==(Direction a, Direction b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(Direction a, Direction b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
