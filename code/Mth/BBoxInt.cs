using Sandbox;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sandcube.Mth;

public struct BBoxInt : IEquatable<BBoxInt>, INbtWritable, INbtStaticReadable<BBoxInt>
{
    [JsonInclude]
    public Vector3Int Mins;

    [JsonInclude]
    public Vector3Int Maxs;

    [JsonIgnore]
    public readonly IEnumerable<Vector3Int> Corners
    {
        get
        {
            yield return new Vector3Int(Mins.x, Mins.y, Mins.z);
            yield return new Vector3Int(Maxs.x, Mins.y, Mins.z);
            yield return new Vector3Int(Maxs.x, Maxs.y, Mins.z);
            yield return new Vector3Int(Mins.x, Maxs.y, Mins.z);
            yield return new Vector3Int(Mins.x, Mins.y, Maxs.z);
            yield return new Vector3Int(Maxs.x, Mins.y, Maxs.z);
            yield return new Vector3Int(Maxs.x, Maxs.y, Maxs.z);
            yield return new Vector3Int(Mins.x, Maxs.y, Maxs.z);
        }
    }

    [JsonIgnore]
    public readonly Vector3 Center => Mins + Size * 0.5f;

    [JsonIgnore]
    public readonly Vector3Int Size => Maxs - Mins;

    [JsonIgnore]
    public readonly Vector3 Extents => Size * 0.5f;

    [JsonIgnore]
    public readonly Vector3 RandomPointInside
    {
        get
        {
            var x = Game.Random.Float(Mins.x, Maxs.x);
            var y = Game.Random.Float(Mins.y, Maxs.y);
            var z = Game.Random.Float(Mins.z, Maxs.z);
            return new(x, y, z);
        }
    }

    [JsonIgnore]
    public readonly Vector3Int RandomPointIntInside
    {
        get
        {
            var x = Game.Random.Float(Mins.x, Maxs.x).RoundToInt();
            var y = Game.Random.Float(Mins.y, Maxs.y).RoundToInt();
            var z = Game.Random.Float(Mins.z, Maxs.z).RoundToInt();
            return new(x, y, z);
        }
    }

    [JsonIgnore]
    public readonly float Volume => Math.Abs(Mins.x - Maxs.x) * Math.Abs(Mins.y - Maxs.y) * Math.Abs(Mins.z - Maxs.z);

    public BBoxInt(in Vector3Int mins, in Vector3Int maxs)
    {
        Mins = default;
        Maxs = default;
        Mins.x = Math.Min(mins.x, maxs.x);
        Mins.y = Math.Min(mins.y, maxs.y);
        Mins.z = Math.Min(mins.z, maxs.z);
        Maxs.x = Math.Max(mins.x, maxs.x);
        Maxs.y = Math.Max(mins.y, maxs.y);
        Maxs.z = Math.Max(mins.z, maxs.z);
    }

    public static BBoxInt FromMinsAndSize(in Vector3Int mins, in Vector3Int size) => new(in mins, mins + size);

    public static BBoxInt FromHeightAndRadius(int height, int radius) =>
        new((Vector3Int.One * -radius).WithZ(0), (Vector3Int.One * radius).WithZ(height));

    public static BBoxInt FromPositionAndRadius(in Vector3Int center, in Vector3Int radius)
    {
        BBoxInt result = default;
        result.Mins = center - radius;
        result.Maxs = center + radius;
        return result;
    }

    public static BBoxInt FromBoxes(IEnumerable<BBoxInt> boxes)
    {
        using IEnumerator<BBoxInt> enumerator = boxes.GetEnumerator();
        if(!enumerator.MoveNext())
        {
            return default;
        }

        BBoxInt result = enumerator.Current;
        while(enumerator.MoveNext())
        {
            BBoxInt point = enumerator.Current;
            result = result.AddBBox(in point);
        }

        return result;
    }

    public static BBoxInt FromPointsAndRadius(IEnumerable<Vector3Int> points, in Vector3Int radius = default)
    {
        using IEnumerator<Vector3Int> enumerator = points.GetEnumerator();
        if(!enumerator.MoveNext())
        {
            return default;
        }

        Vector3Int center = enumerator.Current;
        BBoxInt result = FromPositionAndRadius(in center, in radius);
        while(enumerator.MoveNext())
        {
            center = enumerator.Current;
            BBoxInt point = FromPositionAndRadius(in center, in radius);
            result = result.AddBBox(in point);
        }

        return result;
    }


    public static implicit operator BBox(in BBoxInt bbox) => new(bbox.Mins, bbox.Maxs);


    public readonly IEnumerable<Vector3Int> GetPositions(bool includeMaxs = true) => GetPositions(true, includeMaxs);

    public readonly IEnumerable<Vector3Int> GetPositions(bool includeMins, bool includeMaxs)
    {
        var first = includeMins ? Mins : Mins + 1;
        var last = includeMaxs ? Maxs : Maxs - 1;

        for(int x = first.x; x <= last.x; ++x)
        {
            for(int y = first.y; y <= last.y; ++y)
            {
                for(int z = first.z; z <= last.z; ++z)
                {
                    yield return new(x, y, z);
                }
            }
        }
    }

    public readonly BBox Translate(in Vector3 offset)
    {
        BBox result = this;
        result.Mins += offset;
        result.Maxs += offset;
        return result;
    }

    public readonly BBoxInt Translate(in Vector3Int offset)
    {
        BBoxInt result = this;
        result.Mins += offset;
        result.Maxs += offset;
        return result;
    }

    public readonly BBox Rotate(in Rotation rotation)
    {
        BBox result = this;
        Rotation normal = rotation.Conjugate.Normal;
        Vector3 vector = Vector3.Forward * normal;
        Vector3 vector2 = Vector3.Right * normal;
        Vector3 vector3 = Vector3.Up * normal;
        Vector3 c = 0.5f * (result.Mins + result.Maxs);
        Vector3 vector4 = result.Maxs - c;
        Vector3 vector5 = rotation * c;
        Vector3 vector6 = new(Math.Abs(vector4.x * vector.x) + Math.Abs(vector4.y * vector.y) + Math.Abs(vector4.z * vector.z), Math.Abs(vector4.x * vector2.x) + Math.Abs(vector4.y * vector2.y) + Math.Abs(vector4.z * vector2.z), Math.Abs(vector4.x * vector3.x) + Math.Abs(vector4.y * vector3.y) + Math.Abs(vector4.z * vector3.z));
        result.Mins = vector5 - vector6;
        result.Maxs = vector5 + vector6;
        return result;
    }

    public readonly BBox Transform(in Transform transform) =>
        (this * transform.UniformScale).Rotate(in transform.Rotation).Translate(in transform.Position);

    public readonly bool Contains(in BBoxInt b) =>
        b.Mins.x >= Mins.x && b.Maxs.x < Maxs.x &&
        b.Mins.y >= Mins.y && b.Maxs.y < Maxs.y &&
        b.Mins.z >= Mins.z && b.Maxs.z < Maxs.z;

    public readonly bool Contains(in BBox b) =>
        b.Mins.x >= Mins.x && b.Maxs.x < Maxs.x &&
        b.Mins.y >= Mins.y && b.Maxs.y < Maxs.y &&
        b.Mins.z >= Mins.z && b.Maxs.z < Maxs.z;

    public readonly bool Contains(in Vector3 b) =>
        b.x >= Mins.x && b.x < Maxs.x &&
        b.y >= Mins.y && b.y < Maxs.y &&
        b.z >= Mins.z && b.z < Maxs.z;

    public readonly bool Contains(in Vector3Int b) =>
        b.x >= Mins.x && b.x < Maxs.x &&
        b.y >= Mins.y && b.y < Maxs.y &&
        b.z >= Mins.z && b.z < Maxs.z;

    public readonly bool Overlaps(in BBoxInt b) =>
        Mins.x < b.Maxs.x && b.Mins.x < Maxs.x &&
        Mins.y < b.Maxs.y && b.Mins.y < Maxs.y &&
        Mins.z < b.Maxs.z && b.Mins.z < Maxs.z;

    public readonly bool Overlaps(in BBox b) =>
        Mins.x < b.Maxs.x && b.Mins.x < Maxs.x &&
        Mins.y < b.Maxs.y && b.Mins.y < Maxs.y &&
        Mins.z < b.Maxs.z && b.Mins.z < Maxs.z;

    public readonly BBox GetIntersection(BBox other) => other.GetIntersection(this);
    public readonly BBoxInt GetIntersection(BBoxInt other)
    {
        if(!Overlaps(other))
            return new(Vector3Int.Zero, Vector3Int.Zero);

        BBoxInt result = new()
        {
            Mins = Mins.ComponentMax(other.Mins),
            Maxs = Maxs.ComponentMin(other.Maxs)
        };
        return result;
    }

    public readonly BBox AddPoint(in Vector3 point)
    {
        BBox result = this;
        result.Mins.x = Math.Min(result.Mins.x, point.x);
        result.Mins.y = Math.Min(result.Mins.y, point.y);
        result.Mins.z = Math.Min(result.Mins.z, point.z);
        result.Maxs.x = Math.Max(result.Maxs.x, point.x);
        result.Maxs.y = Math.Max(result.Maxs.y, point.y);
        result.Maxs.z = Math.Max(result.Maxs.z, point.z);
        return result;
    }

    public readonly BBoxInt AddPoint(in Vector3Int point)
    {
        BBoxInt result = this;
        result.Mins.x = Math.Min(result.Mins.x, point.x);
        result.Mins.y = Math.Min(result.Mins.y, point.y);
        result.Mins.z = Math.Min(result.Mins.z, point.z);
        result.Maxs.x = Math.Max(result.Maxs.x, point.x);
        result.Maxs.y = Math.Max(result.Maxs.y, point.y);
        result.Maxs.z = Math.Max(result.Maxs.z, point.z);
        return result;
    }

    public readonly BBox AddBBox(in BBox point)
    {
        BBox result = this;
        result.Mins.x = Math.Min(result.Mins.x, point.Mins.x);
        result.Mins.y = Math.Min(result.Mins.y, point.Mins.y);
        result.Mins.z = Math.Min(result.Mins.z, point.Mins.z);
        result.Maxs.x = Math.Max(result.Maxs.x, point.Maxs.x);
        result.Maxs.y = Math.Max(result.Maxs.y, point.Maxs.y);
        result.Maxs.z = Math.Max(result.Maxs.z, point.Maxs.z);
        return result;
    }

    public readonly BBoxInt AddBBox(in BBoxInt point)
    {
        BBoxInt result = this;
        result.Mins.x = Math.Min(result.Mins.x, point.Mins.x);
        result.Mins.y = Math.Min(result.Mins.y, point.Mins.y);
        result.Mins.z = Math.Min(result.Mins.z, point.Mins.z);
        result.Maxs.x = Math.Max(result.Maxs.x, point.Maxs.x);
        result.Maxs.y = Math.Max(result.Maxs.y, point.Maxs.y);
        result.Maxs.z = Math.Max(result.Maxs.z, point.Maxs.z);
        return result;
    }

    public readonly BBox Grow(in Vector3 skin)
    {
        BBox result = this;
        result.Mins -= skin;
        result.Maxs += skin;
        return result;
    }

    public readonly BBoxInt Grow(in Vector3Int skin)
    {
        BBoxInt result = this;
        result.Mins -= skin;
        result.Maxs += skin;
        return result;
    }

    public readonly BBox Grow(in float skin)
    {
        BBox result = this;
        result.Mins.x -= skin;
        result.Mins.y -= skin;
        result.Mins.z -= skin;
        result.Maxs.x += skin;
        result.Maxs.y += skin;
        result.Maxs.z += skin;
        return result;
    }

    public readonly BBoxInt Grow(in int skin)
    {
        BBoxInt result = this;
        result.Mins.x -= skin;
        result.Mins.y -= skin;
        result.Mins.z -= skin;
        result.Maxs.x += skin;
        result.Maxs.y += skin;
        result.Maxs.z += skin;
        return result;
    }

    public readonly Vector3 ClosestPoint(in Vector3 point)
    {
        return new Vector3(Math.Clamp(point.x, Mins.x, Maxs.x), Math.Clamp(point.y, Mins.y, Maxs.y), Math.Clamp(point.z, Mins.z, Maxs.z));
    }

    public static BBoxInt operator *(in BBoxInt c1, Vector3Int c2) => new(c1.Mins * c2, c1.Maxs * c2);
    public static BBox operator *(in BBoxInt c1, Vector3 c2) => new(c1.Mins * c2, c1.Maxs * c2);

    public static BBox operator *(in BBoxInt c1, float c2) => new(c1.Mins * c2, c1.Maxs * c2);

    public static BBoxInt operator *(in BBoxInt c1, int c2) => new(c1.Mins * c2, c1.Maxs * c2);

    public static BBoxInt operator +(BBoxInt c1, in Vector3Int c2)
    {
        c1.Mins += c2;
        c1.Maxs += c2;
        return c1;
    }

    public static BBoxInt operator -(BBoxInt c1, in Vector3Int c2)
    {
        c1.Mins -= c2;
        c1.Maxs -= c2;
        return c1;
    }

    public static BBox operator +(BBoxInt c1, in Vector3 c2) => new(c1.Mins + c2, c1.Maxs + c2);
    public static BBox operator -(BBoxInt c1, in Vector3 c2) => new(c1.Mins - c2, c1.Maxs - c2);

    public readonly bool Trace(in Ray ray, float distance, out float hitDistance)
    {
        hitDistance = 0f;
        int num = -1;
        float num2 = -1f;
        float num3 = 1f;
        Vector3 vector = ray.Forward.Normal * distance;
        bool flag = false;
        for(int i = 0; i < 6; i++)
        {
            float num4;
            float num5;
            if(i >= 3)
            {
                num4 = ray.Position[i - 3] - Maxs[i - 3];
                num5 = num4 + vector[i - 3];
            }
            else
            {
                num4 = 0f - ray.Position[i] + Mins[i];
                num5 = num4 - vector[i];
            }

            if(num4 > 0f && num5 > 0f)
            {
                return false;
            }

            if(num4 <= 0f && num5 <= 0f)
            {
                continue;
            }

            if(num4 > 0f)
            {
                flag = false;
            }

            float num6;
            if(num4 > num5)
            {
                num6 = num4;
                if(num6 < 0f)
                {
                    num6 = 0f;
                }

                num6 /= num4 - num5;
                if(num6 > num2)
                {
                    num2 = num6;
                    num = i;
                }

                continue;
            }

            num6 = num4 / (num4 - num5);
            if(num6 < num3)
            {
                num3 = num6;
                if(num < 0)
                {
                    num = i;
                }
            }
        }

        hitDistance = distance * num2;
        return flag || (num2 < num3 && num2 >= 0f);
    }

    public override readonly string ToString() => $"mins {Mins}, maxs {Maxs}";

    public static bool operator ==(in BBoxInt left, in BBoxInt right) => left.Equals(right);

    public static bool operator !=(in BBoxInt left, in BBoxInt right) => !(left == right);

    public override readonly bool Equals(object? obj) => obj is BBoxInt o && Equals(o);

    public readonly bool Equals(BBoxInt other) => Mins == other.Mins && Maxs == other.Maxs;

    public readonly bool AlmostEqual(in BBox other, float delta = 0.0001f) =>
        Mins.AlmostEqual(other.Mins, delta) && Maxs.AlmostEqual(other.Maxs, delta);

    public override readonly int GetHashCode() => HashCode.Combine(Mins, Maxs);


    public readonly BinaryTag Write() => new ListTag()
    {
        Mins.x,
        Mins.y,
        Mins.z,
        Maxs.x,
        Maxs.y,
        Maxs.z
    };

    public static BBoxInt Read(BinaryTag tag)
    {
        var listTag = tag.To<ListTag>();
        return new(new Vector3Int(listTag.Get<int>(0), listTag.Get<int>(1), listTag.Get<int>(2)),
            new Vector3Int(listTag.Get<int>(3), listTag.Get<int>(4), listTag.Get<int>(5)));
    }
}