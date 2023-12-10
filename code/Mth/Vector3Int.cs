using Sandbox;
using Sandcube.Mth.Enums;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Sandcube.Mth;

public struct Vector3Int
{
    public static readonly Vector3Int One = new(1);
    public static readonly Vector3Int Zero = new(0);
    public static readonly Vector3Int Forward = (Vector3Int)Vector3.Forward;
    public static readonly Vector3Int Backward = (Vector3Int)Vector3.Backward;
    public static readonly Vector3Int Left = (Vector3Int)Vector3.Left;
    public static readonly Vector3Int Right = (Vector3Int)Vector3.Right;
    public static readonly Vector3Int Up = (Vector3Int)Vector3.Up;
    public static readonly Vector3Int Down = (Vector3Int)Vector3.Down;


#pragma warning disable IDE1006 // Naming Styles - We need to use the same naming as facepunch
    public int x { readonly get; set; }
    public int y { readonly get; set; }
    public int z { readonly get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Int(int all)
    {
        this.x = all;
        this.y = all;
        this.z = all;
    }

    public Vector3Int(Vector3Int other)
    {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }


    public readonly Vector3 Normal
    {
        get
        {
            if(this == Zero)
                return this;

            return this / Length;
        }
    }

    public readonly float Length => MathF.Sqrt(LengthSquared);

    [JsonIgnore]
    public readonly float LengthSquared => x * x + y * y + z * z;


    public readonly Vector3Int WithX(int x) => new(x, this.y, this.z);
    public readonly Vector3Int WithY(int y) => new(this.x, y, this.z);
    public readonly Vector3Int WithZ(int z) => new(this.x, this.y, z);

    public readonly Vector3 WithX(float x) => new(x, this.y, this.z);
    public readonly Vector3 WithY(float y) => new(this.x, y, this.z);
    public readonly Vector3 WithZ(float z) => new(this.x, this.y, z);

    public readonly Vector3 ClampLength(float maxLength) => ((Vector3)this).ClampLength(maxLength);
    public readonly Vector3 ClampLength(float minLength, float maxLength) => ((Vector3)this).ClampLength(minLength, maxLength);

    public readonly Vector3Int Clamp(Vector3Int otherMin, Vector3Int otherMax) =>
        new(Math.Clamp(x, otherMin.x, otherMax.x), Math.Clamp(y, otherMin.y, otherMax.y), Math.Clamp(z, otherMin.z, otherMax.z));
    public readonly Vector3 Clamp(Vector3 otherMin, Vector3 otherMax) =>
        new(Math.Clamp(x, otherMin.x, otherMax.x), Math.Clamp(y, otherMin.y, otherMax.y), Math.Clamp(z, otherMin.z, otherMax.z));
    public readonly Vector3Int Clamp(int otherMin, int otherMax) => Clamp((Vector3Int)otherMin, (Vector3Int)otherMax);
    public readonly Vector3 Clamp(float otherMin, float otherMax) => Clamp((Vector3)otherMin, (Vector3)otherMax);

    public readonly Vector3Int ComponentMin(Vector3Int other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public readonly Vector3 ComponentMin(Vector3 other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public static Vector3Int Min(Vector3Int a, Vector3Int b) => a.ComponentMin(b);
    public static Vector3 Min(Vector3 a, Vector3 b) => a.ComponentMin(b);

    public readonly Vector3Int ComponentMax(Vector3Int other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public readonly Vector3 ComponentMax(Vector3 other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public static Vector3Int Max(Vector3Int a, Vector3Int b) => a.ComponentMax(b);
    public static Vector3 Max(Vector3 a, Vector3 b) => a.ComponentMax(b);

    public readonly Vector3 LerpTo(Vector3 target, float frac, bool clamp = true) => Vector3.Lerp(this, target, frac, clamp);

    public static Vector3Int Cross(Vector3Int a, Vector3Int b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    public static Vector3 Cross(Vector3 a, Vector3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    public readonly Vector3Int Cross(Vector3Int b) => Cross(this, b);
    public readonly Vector3 Cross(Vector3 b) => Cross(this, b);

    public static int Dot(Vector3Int a, Vector3Int b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public readonly int Dot(Vector3Int b) => Dot(this, b);
    public readonly float Dot(Vector3 b) => Dot(this, b);

    public static float DistanceBetween(Vector3Int a, Vector3 b) => (b - a).Length;
    public static float DistanceBetween(Vector3 a, Vector3Int b) => (b - a).Length;
    public readonly float Distance(Vector3 target) => DistanceBetween(this, target);

    public static float DistanceBetweenSquared(Vector3Int a, Vector3 b) => (b - a).LengthSquared;
    public static float DistanceBetweenSquared(Vector3 a, Vector3Int b) => (b - a).LengthSquared;
    public readonly float DistanceSquared(Vector3 target) => DistanceBetweenSquared(this, target);

    public readonly Vector3 Approach(float length, float amount) => Normal * Length.Approach(length, amount);

    public readonly Vector3Int Abs() => new(Math.Abs(x), Math.Abs(y), Math.Abs(z));

    public static Vector3 Reflect(Vector3 direction, Vector3 normal) => direction - 2f * Dot(direction, normal) * normal;
    public static Vector3Int Reflect(Vector3Int direction, Vector3Int normal) => direction - 2 * Dot(direction, normal) * normal;

    public static Vector3Int VectorPlaneProject(Vector3Int v, Vector3Int planeNormal) => v - v.ProjectOnNormal(planeNormal);
    public static Vector3 VectorPlaneProject(Vector3 v, Vector3 planeNormal) => v - v.ProjectOnNormal(planeNormal);
    public readonly Vector3Int ProjectOnNormal(Vector3Int normal) => normal * Dot(this, normal);
    public readonly Vector3 ProjectOnNormal(Vector3 normal) => normal * Dot(this, normal);

    public static void Sort(ref Vector3Int min, ref Vector3Int max)
    {
        Vector3Int vector = new(Math.Min(min.x, max.x), Math.Min(min.y, max.y), Math.Min(min.z, max.z));
        Vector3Int vector2 = new(Math.Max(min.x, max.x), Math.Max(min.y, max.y), Math.Max(min.z, max.z));
        min = vector;
        max = vector2;
    }

    public readonly bool AlmostEqual(Vector3 v, float delta = 0.0001f)
    {
        if(Math.Abs(x - v.x) > delta)
            return false;

        if(Math.Abs(y - v.y) > delta)
            return false;

        if(Math.Abs(z - v.z) > delta)
            return false;

        return true;
    }

    public static Vector3 CubicBeizer(Vector3 source, Vector3 target, Vector3 sourceTangent, Vector3 targetTangent, float t) =>
        Vector3.CubicBeizer(source, target, sourceTangent, targetTangent, t);

    public readonly Vector3Int SubtractDirection(Vector3Int direction, int strength = 1) => this - direction * Dot(direction) * strength;
    public readonly Vector3 SubtractDirection(Vector3 direction, float strength = 1f) => this - direction * Dot(direction) * strength;

    public readonly Vector3 SnapToGrid(float gridSize, bool sx = true, bool sy = true, bool sz = true) => ((Vector3)this).SnapToGrid(gridSize, sx, sy, sz);

    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float deltaTime) =>
        Vector3.SmoothDamp(current, target, ref velocity, smoothTime, deltaTime);


    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
    }

    public static Vector3Int Read(BinaryReader reader) => new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());


    public static float GetAngle(Vector3 v1, Vector3 v2) => Vector3.GetAngle(v1, v2);
    public readonly float Angle(Vector3 v2) => GetAngle(this, v2);
    public static Angles VectorAngle(Vector3 vec) => Vector3.VectorAngle(vec);

    public readonly Vector3 AddClamped(Vector3 toAdd, float maxLength) => ((Vector3)this).AddClamped(toAdd, maxLength);

    public readonly Vector3 RotateAround(Vector3 center, Rotation rot) => ((Vector3)this).RotateAround(center, rot);

    public readonly int GetAxis(Axis axis)
    {
        if(axis == Axis.X)
            return x;
        if(axis == Axis.Y)
            return y;
        return z;
    }

    public void SetAxis(Axis axis, int value)
    {
        if(axis == Axis.X)
            x = value;
        else if(axis == Axis.Y)
            y = value;
        else
            z = value;
    }

    public readonly Vector3Int WithAxis(Axis axis, int value)
    {
        if(axis == Axis.X)
            return WithX(value);
        if(axis == Axis.Y)
            return WithY(value);
        return WithZ(value);
    }

    public readonly Vector3Int WithAxes(Func<Axis, int, int> axisChanger)
    {
        return new Vector3Int(axisChanger.Invoke(Axis.X, x), axisChanger.Invoke(Axis.Y, y), axisChanger.Invoke(Axis.Z, z));
    }

    public readonly Vector3Int WithAxes(Func<int, int> axisChanger) => WithAxes((_, v) => axisChanger.Invoke(v));

    public bool IsAnyAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, GetAxis(axis)))
                return true;
        }
        return false;
    }
    public bool IsAnyAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public bool IsEveryAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, GetAxis(axis)))
                return false;
        }
        return true;
    }
    public bool IsEveryAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public void EachAxis(Action<Axis, int> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, GetAxis(axis));
    }
    public void EachAxis(Action<int> action) => EachAxis((a, v) => action.Invoke(v));


    public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3Int operator -(Vector3Int vector) => new(-vector.x, -vector.y, -vector.z);
    public static Vector3Int operator *(Vector3Int vector, int value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3Int operator *(int value, Vector3Int vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(Vector3Int vector, float value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(float value, Vector3Int vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3Int operator *(Vector3Int a, Vector3Int b) => new(a.x * b.x, a.y * b.y, a.z * b.x);
    public static Vector3 operator *(Vector3Int a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.x);
    public static Vector3 operator *(Vector3 a, Vector3Int b) => new(a.x * b.x, a.y * b.y, a.z * b.x);
    public static Vector3 operator *(Vector3Int vector, Rotation value) => System.Numerics.Vector3.Transform(vector, value);
    public static Vector3 operator /(Vector3Int vector, float value) => new(vector.x / value, vector.y / value, vector.z / value);
    public static Vector3Int operator /(Vector3Int a, Vector3Int b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 operator /(Vector3Int a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 operator /(Vector3 a, Vector3Int b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3Int operator %(Vector3Int a, Vector3Int b) => new(a.x % b.x, a.y % b.y, a.z % b.z);


    public static implicit operator Vector3(Vector3Int vector) => new(vector.x, vector.y, vector.z);
    public static implicit operator System.Numerics.Vector3(Vector3Int vector) => new(vector.x, vector.y, vector.z);
    public static explicit operator Vector3Int(Vector3 vector) => new((int)vector.x, (int)vector.y, (int)vector.z);
    public static explicit operator Vector3Int(Vector2 vector) => new((int)vector.x, (int)vector.y, 0);
    public static explicit operator Vector3Int(Vector4 vector) => new((int)vector.x, (int)vector.y, (int)vector.z);
    public static explicit operator Vector3Int(Color color) => (Vector3Int)(Vector3)color;
    public static explicit operator Vector3Int(System.Numerics.Vector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
    public static implicit operator Vector3Int(int all) => new(all);

    public static bool operator ==(Vector3Int left, Vector3Int right) => left.x == right.x && left.y == right.y && left.z == right.z;
    public static bool operator !=(Vector3Int left, Vector3Int right) => !(left == right);

    public override readonly bool Equals(object? obj)
    {
        if(obj is Vector3Int vector)
            return Equals(vector);
        return false;
    }

    public readonly bool Equals(Vector3 vector)
    {
        return this == vector;
    }

    public override readonly int GetHashCode() => HashCode.Combine(x, y, z);


    public override readonly string ToString() => ToString("0.####");

    public readonly string ToString(string? valueFormat)
    {
        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(2, 3);
        defaultInterpolatedStringHandler.AppendFormatted(x, valueFormat);
        defaultInterpolatedStringHandler.AppendLiteral(",");
        defaultInterpolatedStringHandler.AppendFormatted(y, valueFormat);
        defaultInterpolatedStringHandler.AppendLiteral(",");
        defaultInterpolatedStringHandler.AppendFormatted(z, valueFormat);
        return defaultInterpolatedStringHandler.ToStringAndClear();
    }
}
