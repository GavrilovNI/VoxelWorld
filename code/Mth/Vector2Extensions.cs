using VoxelWorld.Mth.Enums;
using System;

namespace VoxelWorld.Mth;

public static class Vector2Extensions
{
    public static Vector2 ClampLength(this Vector2 vector, float maxLength)
	{
		if((double)vector.LengthSquared <= 0.0)
			return Vector2.Zero;

		if(vector.LengthSquared < maxLength * maxLength)
			return vector;

		return vector.Normal * maxLength;
	}

    public static Vector2 ClampLength(this Vector2 vector, float minLength, float maxLength)
    {
        float num = minLength * minLength;
        float num2 = maxLength * maxLength;
        float lengthSquared = vector.LengthSquared;
        if(lengthSquared <= 0f)
            return Vector2.Zero;

        if(lengthSquared <= num)
            return vector.Normal * minLength;

        if(lengthSquared >= num2)
            return vector.Normal * maxLength;

        return vector;
    }

    public static Vector2IntB Floor(this Vector2 vector) => new((int)MathF.Floor(vector.x), (int)MathF.Floor(vector.y));
    public static Vector2IntB Ceiling(this Vector2 vector) => new((int)MathF.Ceiling(vector.x), (int)MathF.Ceiling(vector.y));
    public static Vector2IntB Round(this Vector2 vector) => new((int)MathF.Round(vector.x), (int)MathF.Round(vector.y));

    public static Vector2 Divide(this Vector2 dividend, Vector2 divisor) => new(dividend.x / divisor.x, dividend.y / divisor.y);

    public static float GetAxis(this Vector2 vector, Axis axis)
    {
        if(axis == Axis.X)
            return vector.x;
        if(axis == Axis.Y)
            return vector.y;
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }

    public static Vector2 WithAxis(this Vector2 vector, Axis axis, float value)
    {
        if(axis == Axis.X)
            return vector.WithX(value);
        if(axis == Axis.Y)
            return vector.WithY(value);
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }
    public static Vector2 WithAxes(this Vector2 vector, Func<Axis, float, float> axisChanger) =>
        new(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y));

    public static Vector2 WithAxes(this Vector2 vector, Func<float, float> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));
    
    public static Vector2Int WithAxes(this Vector2 vector, Func<Axis, float, int> axisChanger) =>
        new(axisChanger.Invoke(Axis.X, vector.x), axisChanger.Invoke(Axis.Y, vector.y));

    public static Vector2Int WithAxes(this Vector2 vector, Func<float, int> axisChanger) => vector.WithAxes((_, v) => axisChanger.Invoke(v));


    public static bool IsAnyAxis(this Vector2 vector, Func<Axis, float, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, vector.GetAxis(axis)))
                return true;
        }
        return false;
    }
    public static bool IsAnyAxis(this Vector2 vector, Func<float, bool> predicate) => vector.IsAnyAxis((a, v) => predicate.Invoke(v));

    public static bool IsEveryAxis(this Vector2 vector, Func<Axis, float, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, vector.GetAxis(axis)))
                return false;
        }
        return true;
    }
    public static bool IsEveryAxis(this Vector2 vector, Func<float, bool> predicate) => vector.IsEveryAxis((a, v) => predicate.Invoke(v));

    public static void EachAxis(this Vector2 vector, Action<Axis, float> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, vector.GetAxis(axis));
    }
    public static void EachAxis(this Vector2 vector, Action<float> action) => vector.EachAxis((a, v) => action.Invoke(v));
}
