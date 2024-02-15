using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandcube.Mth;

public static class BBoxExtensions
{
    public static BBox Expanded(this BBox bbox, Vector3 value)
    {
        var result = bbox;
        result.Mins -= value;
        result.Maxs += value;
        return result;
    }

    public static BBox AddOrCreate(this BBox? @this, Vector3 point)
    {
        if(@this.HasValue)
            return @this.Value.AddPoint(point);
        return new(point, point);
    }

    public static BBox AddOrCreate(this BBox? @this, BBox bbox)
    {
        if(@this.HasValue)
            return @this.Value.AddBBox(bbox);
        return bbox;
    }
}
