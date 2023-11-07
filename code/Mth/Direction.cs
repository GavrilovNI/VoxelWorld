using Sandcube.Components;
using System.Collections.Generic;

namespace Sandcube.Mth;

public sealed record class Direction
{
    [CustomEnumValue]
    public static readonly Direction Forward = new("forward", Axis.X, AxisDirection.Positive);
    [CustomEnumValue]
    public static readonly Direction Backward = new("backward", Axis.X, AxisDirection.Negative);
    [CustomEnumValue]
    public static readonly Direction Left = new("left", Axis.Y, AxisDirection.Positive);
    [CustomEnumValue]
    public static readonly Direction Right = new("right", Axis.Y, AxisDirection.Negative);
    [CustomEnumValue]
    public static readonly Direction Up = new("up", Axis.Z, AxisDirection.Positive);
    [CustomEnumValue]
    public static readonly Direction Down = new("down", Axis.Z, AxisDirection.Negative);

    public static readonly IReadOnlyList<Direction> All = new List<Direction>() { Forward, Backward, Left, Right, Up, Down }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Horizontal = new List<Direction>() { Forward, Backward, Left, Right }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Vertical = new List<Direction>() { Up, Down }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Positive = new List<Direction>() { Forward, Left, Up }.AsReadOnly();
    public static readonly IReadOnlyList<Direction> Negative = new List<Direction>() { Backward, Right, Down }.AsReadOnly();


    public string Name { get; init; }
    public Axis Axis { get; init; }
    public AxisDirection AxisDirection { get; init; }
    public Vector3Int Normal { get; init; }

    [Obsolete("For serialization only", true)]
    public Direction()
    {

    }

    private Direction(string name, Axis axis, AxisDirection axisDirection)
    {
        Name = name;
        Axis = axis;
        AxisDirection = axisDirection;
        Normal = Axis.PositiveNormal * AxisDirection.Normal;
    }

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
    public static Direction ClosestTo(Vector3 direction) => ClosestTo(direction, Direction.Up);

    public static implicit operator Vector3Int(Direction direction) => direction.Normal;

    public static Vector3Int operator *(Direction direction, int value) => direction.Normal * value;
    public static Vector3Int operator *(int value, Direction direction) => direction.Normal * value;
    public static Vector3 operator *(Direction direction, float value) => direction.Normal * value;
    public static Vector3 operator *(float value, Direction direction) => direction.Normal * value;

}
