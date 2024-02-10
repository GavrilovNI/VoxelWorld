using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sandcube.Mth.Enums;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
[JsonConverter(typeof(CustomEnumJsonConverter))]
public sealed class RightAngle : CustomEnum<RightAngle>, ICustomEnum<RightAngle>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly RightAngle Angle0 = new(0, "Angle0");
    public static readonly RightAngle Angle90 = new(1, "Angle90");
    public static readonly RightAngle Angle180 = new(2, "Angle180");
    public static readonly RightAngle Angle270 = new(3, "Angle270");

    public static IReadOnlyList<RightAngle> All { get; } = new List<RightAngle>() { Angle0, Angle90, Angle180, Angle270 }.AsReadOnly();

    public int Angle => Ordinal * 90;

    [Obsolete("For serialization only", true)]
    public RightAngle()
    {
    }

    private RightAngle(int ordinal, string name) : base(ordinal, name)
    {
    }

    public static RightAngle ClosestTo(float angle)
    {
        angle = (angle % 360 + 360) % 360;
        int ordinal = (int)MathF.Round(angle / 90f);
        return All[ordinal];
    }

    public static RightAngle FromTo(Direction from, Direction to)
    {
        if(from == to)
            return RightAngle.Angle0;
        if(from.GetOpposite() == to)
            return RightAngle.Angle180;
        return RightAngle.Angle90;
    }

    public static RightAngle FromTo(Direction from, Direction to, Direction lookDirection) // TODO: optimize
    {
        if(lookDirection.Axis == from.Axis || lookDirection.Axis == to.Axis)
            throw new ArgumentException($"{nameof(lookDirection)} should not lie on the same axis as {nameof(from)} or {nameof(to)}");

        if(from.Axis == to.Axis)
            return from == to ? RightAngle.Angle0 : RightAngle.Angle180;

        from = from.Rotate(RightAngle.Angle90, lookDirection);
        if(from == to)
            return RightAngle.Angle90;
        return RightAngle.Angle270;
    }

    public static RightAngle FromNonClampedOrdinal(int ordinal) => All[(ordinal % 4 + 4) % 4];

    public RightAngle Rotate() => FromNonClampedOrdinal(Ordinal + 1);
    public RightAngle RotateCounterclockwise() => FromNonClampedOrdinal(Ordinal + 3);
    public RightAngle Rotate180() => FromNonClampedOrdinal(Ordinal + 2);

    public static bool TryParse(string name, out RightAngle value) => TryParse(All, name, out value);

    public static RightAngle operator *(RightAngle rotation, int value) => FromNonClampedOrdinal(rotation.Ordinal * value);
    public static RightAngle operator *(int value, RightAngle rotation) => FromNonClampedOrdinal(rotation.Ordinal * value);

    public static RightAngle operator +(RightAngle a, RightAngle b) => FromNonClampedOrdinal(a.Ordinal + b.Ordinal);
    public static RightAngle operator -(RightAngle a, RightAngle b) => FromNonClampedOrdinal(a.Ordinal - b.Ordinal);
    public static RightAngle operator -(RightAngle rotation) => FromNonClampedOrdinal(rotation.Ordinal + 2);

    public static bool operator ==(RightAngle a, RightAngle b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(RightAngle a, RightAngle b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
