using System;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Mth;

public static class Vector2IntExtensions
{
    [Obsolete("TODO: remove when implicit operator appear")]
    public static Vector2 ToFloatVector(this Vector2Int from) => new(from.x, from.y);

    public static Vector2Int Divide(this Vector2Int dividend, Vector2Int divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y);
    public static Vector2 Divide(this Vector2Int dividend, Vector2 divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y);


    public static int GetAxis(this Vector2Int vector, Axis axis)
    {
        if(axis == Axis.X)
            return vector.x;
        if(axis == Axis.Y)
            return vector.y;
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }

    public static Vector2Int WithAxis(this Vector2Int vector, Axis axis, int value)
    {
        if(axis == Axis.X)
            return vector.WithX(value);
        if(axis == Axis.Y)
            return vector.WithY(value);
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }
    public static Vector2Int WithAxes(this Vector2Int vector, Func<Axis, int, int> axisChanger) =>
        new Vector2Int(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y));

    public static Vector2Int WithAxes(this Vector2Int vector, Func<int, int> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));

    public static Vector2 WithAxes(this Vector2Int vector, Func<Axis, int, float> axisChanger) =>
        new(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y));

    public static Vector2 WithAxes(this Vector2Int vector, Func<int, float> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));


    public static bool IsAnyAxis(this Vector2Int vector, Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.XY)
        {
            if(predicate.Invoke(axis, vector.GetAxis(axis)))
                return true;
        }
        return false;
    }
    public static bool IsAnyAxis(this Vector2Int vector, Func<int, bool> predicate) => vector.IsAnyAxis((a, v) => predicate.Invoke(v));

    public static bool IsEveryAxis(this Vector2Int vector, Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.XY)
        {
            if(!predicate.Invoke(axis, vector.GetAxis(axis)))
                return false;
        }
        return true;
    }
    public static bool IsEveryAxis(this Vector2Int vector, Func<int, bool> predicate) => vector.IsEveryAxis((a, v) => predicate.Invoke(v));

    public static void EachAxis(this Vector2Int vector, Action<Axis, int> action)
    {
        foreach(var axis in Axis.XY)
            action.Invoke(axis, vector.GetAxis(axis));
    }
    public static void EachAxis(this Vector2Int vector, Action<int> action) => vector.EachAxis((a, v) => action.Invoke(v));
}
