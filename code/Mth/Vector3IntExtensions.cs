using Sandbox;
using System;
using System.Collections.Generic;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Mth;

public static class Vector3IntExtensions
{
    [Obsolete("TODO: remove when implicit operator appear")]
    public static Vector3 ToFloatVector(this Vector3Int from) => new(from.x, from.y, from.z);

    public static readonly Comparer<Vector3Int> XYZIterationComparer = Comparer<Vector3Int>.Create((a, b) =>
    {
        var result = a.z - b.z;
        if(result != 0)
            return result;

        result = a.y - b.y;
        if(result != 0)
            return result;

        return a.x - b.x;
    });

    public static float SignedAngle(this Vector3Int from, in Vector3 to, in Vector3 axis) =>
        from.ToFloatVector().SignedAngle(to, axis);

    public static Vector3 ProjectOnPlane(this Vector3Int vector, in Vector3 planeNormal)
    {
        float sqrMag = planeNormal.Dot(planeNormal);
        if(sqrMag.AlmostEqual(0))
            return vector.ToFloatVector();

        var dot = vector.ToFloatVector().Dot(planeNormal);
        return new Vector3(vector.x - planeNormal.x * dot / sqrMag,
            vector.y - planeNormal.y * dot / sqrMag,
            vector.z - planeNormal.z * dot / sqrMag);
    }

    public static Vector3Int Divide(this Vector3Int dividend, Vector3Int divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y, dividend.z / divisor.z);
    public static Vector3 Divide(this Vector3Int dividend, Vector3 divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y, dividend.z / divisor.z);

    public static int GetAxis(this Vector3Int vector, Axis axis)
    {
        if(axis == Axis.X)
            return vector.x;
        if(axis == Axis.Y)
            return vector.y;
        return vector.z;
    }

    public static Vector3Int WithAxis(this Vector3Int vector, Axis axis, int value)
    {
        if(axis == Axis.X)
            return vector.WithX(value);
        if(axis == Axis.Y)
            return vector.WithY(value);
        return vector.WithZ(value);
    }

    public static Vector3Int WithAxes(this Vector3Int vector, Func<Axis, int, int> axisChanger) =>
        new(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y), axisChanger.Invoke(Axis.Z, vector.z));
    public static Vector3Int WithAxes(this Vector3Int vector, Func<int, int> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));
    
    public static Vector3 WithAxes(this Vector3Int vector, Func<Axis, int, float> axisChanger) =>
        new(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y), axisChanger.Invoke(Axis.Z, vector.z));

    public static Vector3 WithAxes(this Vector3Int vector, Func<int, float> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));


    public static bool IsAnyAxis(this Vector3Int vector, Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, vector.GetAxis(axis)))
                return true;
        }
        return false;
    }
    public static bool IsAnyAxis(this Vector3Int vector, Func<int, bool> predicate) => vector.IsAnyAxis((a, v) => predicate.Invoke(v));

    public static bool IsEveryAxis(this Vector3Int vector, Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, vector.GetAxis(axis)))
                return false;
        }
        return true;
    }
    public static bool IsEveryAxis(this Vector3Int vector, Func<int, bool> predicate) => vector.IsEveryAxis((a, v) => predicate.Invoke(v));

    public static void EachAxis(this Vector3Int vector, Action<Axis, int> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, vector.GetAxis(axis));
    }
    public static void EachAxis(this Vector3Int vector, Action<int> action) => vector.EachAxis((a, v) => action.Invoke(v));


    public static IEnumerable<Vector3Int> GetPositionsFromZero(this Vector3Int vector, bool includeMaxs = true) => vector.GetPositionsFromZero(true, includeMaxs);

    public static IEnumerable<Vector3Int> GetPositionsFromZero(this Vector3Int vector, bool includeZero, bool includeMaxs)
    {
        var first = includeZero ? Vector3Int.Zero : Vector3Int.One;
        var last = includeMaxs ? vector : vector - 1;

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
}
