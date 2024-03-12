

namespace Sandcube.Mth;

public static class BBoxExtensions
{
    public static bool AlmostEqual(this BBox @this, in BBoxInt other, float delta = 0.0001f) => other.AlmostEqual(@this, delta);

    public static BBoxInt Floor(this BBox bbox) => new(bbox.Mins.Floor(), bbox.Maxs.Floor());
    public static BBoxInt Round(this BBox bbox) => new(bbox.Mins.Round(), bbox.Maxs.Round());
    public static BBoxInt Ceiling(this BBox bbox) => new(bbox.Mins.Ceiling(), bbox.Maxs.Ceiling());
    public static BBoxInt ExpandedToInt(this BBox bbox) => new(bbox.Mins.Floor(), bbox.Maxs.Ceiling());

    public static BBox Grow(this BBox bbox, in Vector3 value)
    {
        var result = bbox;
        result.Mins -= value;
        result.Maxs += value;
        return result;
    }

    public static BBox AddOrCreate(this BBox? @this, in Vector3 point)
    {
        if(@this.HasValue)
            return @this.Value.AddPoint(point);
        return new(point, point);
    }

    public static BBox AddOrCreate(this BBox? @this, in BBox bbox)
    {
        if(@this.HasValue)
            return @this.Value.AddBBox(bbox);
        return bbox;
    }

    public static BBox GetIntersection(this BBox @this, BBox other)
    {
        if(!@this.Overlaps(other))
            return new(Vector3Int.Zero, Vector3Int.Zero);

        BBox result = new()
        {
            Mins = @this.Mins.ComponentMax(other.Mins),
            Maxs = @this.Maxs.ComponentMin(other.Maxs)
        };
        return result;
    }
}
