using Sandbox;
using Sandcube.Mth.Enums;
using System;

namespace Sandcube.Mth;

public static class Vector3Extensions
{
    public static Vector3 ProjectOnPlane(this Vector3 vector, in Vector3 planeNormal)
    {
        float sqrMag = planeNormal.Dot(planeNormal);
        if(sqrMag.AlmostEqual(0))
            return vector;

        var dot = vector.Dot(planeNormal);
        return new Vector3(vector.x - planeNormal.x * dot / sqrMag,
            vector.y - planeNormal.y * dot / sqrMag,
            vector.z - planeNormal.z * dot / sqrMag);
    }

    public static Vector3Int Floor(this Vector3 vector) => new((int)MathF.Floor(vector.x), (int)MathF.Floor(vector.y), (int)MathF.Floor(vector.z));
    public static Vector3Int Ceiling(this Vector3 vector) => new((int)MathF.Ceiling(vector.x), (int)MathF.Ceiling(vector.y), (int)MathF.Ceiling(vector.z));
    public static Vector3Int Round(this Vector3 vector) => new((int)MathF.Round(vector.x), (int)MathF.Round(vector.y), (int)MathF.Round(vector.z));

    public static Vector3 Divide(this Vector3 dividend, Vector3 divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y, dividend.z / divisor.z);

    public static float GetAxis(this Vector3 vector, Axis axis)
    {
        if(axis == Axis.X)
            return vector.x;
        if(axis == Axis.Y)
            return vector.y;
        return vector.z;
    }

    public static Vector3 WithAxis(this Vector3 vector, Axis axis, float value)
    {
        if(axis == Axis.X)
            return vector.WithX(value);
        if(axis == Axis.Y)
            return vector.WithY(value);
        return vector.WithZ(value);
    }
    public static Vector3 WithAxes(this Vector3 vector, Func<Axis, float, float> axisChanger)
    {
        return new Vector3(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y), axisChanger.Invoke(Axis.Z, vector.z));
    }

    public static Vector3 WithAxes(this Vector3 vector, Func<float, float> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));

    public static bool IsAnyAxis(this Vector3 vector, Func<Axis, float, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, vector.GetAxis(axis)))
                return true;
        }
        return false;
    }
    public static bool IsAnyAxis(this Vector3 vector, Func<float, bool> predicate) => vector.IsAnyAxis((a, v) => predicate.Invoke(v));

    public static bool IsEveryAxis(this Vector3 vector, Func<Axis, float, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, vector.GetAxis(axis)))
                return false;
        }
        return true;
    }
    public static bool IsEveryAxis(this Vector3 vector, Func<float, bool> predicate) => vector.IsEveryAxis((a, v) => predicate.Invoke(v));

    public static void EachAxis(this Vector3 vector, Action<Axis, float> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, vector.GetAxis(axis));
    }
    public static void EachAxis(this Vector3 vector, Action<float> action) => vector.EachAxis((a, v) => action.Invoke(v));
}
