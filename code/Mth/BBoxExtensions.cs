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
}
