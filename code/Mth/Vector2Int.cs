using Sandbox;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandcube.Mth;

[JsonConverter(typeof(Vector2IntJsonConverter))]
public struct Vector2Int : IEquatable<Vector2Int>, IParsable<Vector2Int>, INbtWritable, INbtStaticReadable<Vector2Int>, IBinaryWritable, IBinaryStaticReadable<Vector2Int>
{
    public static readonly Vector2Int One = new(1);
    public static readonly Vector2Int Zero = new(0);
    public static readonly Vector2Int Left = (Vector2Int)Vector2.Left;
    public static readonly Vector2Int Right = (Vector2Int)Vector2.Right;
    public static readonly Vector2Int Up = (Vector2Int)Vector2.Up;
    public static readonly Vector2Int Down = (Vector2Int)Vector2.Down;

    public static readonly Comparer<Vector2Int> XYZIterationComparer = Comparer<Vector2Int>.Create((a, b) =>
    {
        var result = a.y - b.y;
        if(result != 0)
            return result;

        return a.x - b.x;
    });

#pragma warning disable IDE1006 // Naming Styles - We need to use the same naming as facepunch
    public int x { readonly get; set; }
    public int y { readonly get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int(int all)
    {
        this.x = all;
        this.y = all;
    }

    public Vector2Int(Vector2Int other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    [JsonIgnore]
    public readonly float Length => MathF.Sqrt(LengthSquared);

    [JsonIgnore]
    public readonly float LengthSquared => x * x + y * y;

    [JsonIgnore]
    public readonly Vector2Int Perpendicular => new(0 - y, x);

    [JsonIgnore]
    public readonly Vector2 Normal => this == Zero ? Zero : this / Length;

    [JsonIgnore]
    public readonly float Degrees => MathF.Atan2(x, 0 - y).RadianToDegree().NormalizeDegrees();


    public readonly Vector2Int WithX(int x) => new(x, this.y);
    public readonly Vector2Int WithY(int y) => new(this.x, y);
    public readonly Vector3Int WithZ(int z) => new(this.x, this.y, z);

    public readonly Vector2 WithX(float x) => new(x, this.y);
    public readonly Vector2 WithY(float y) => new(this.x, y);
    public readonly Vector3 WithZ(float z) => new(this.x, this.y, z);


    public readonly Vector2 ClampLength(float maxLength) => ((Vector2)this).ClampLength(maxLength);
    public readonly Vector2 ClampLength(float minLength, float maxLength) => ((Vector2)this).ClampLength(minLength, maxLength);

    public readonly Vector2Int Clamp(Vector2Int otherMin, Vector2Int otherMax) =>
        new(Math.Clamp(x, otherMin.x, otherMax.x), Math.Clamp(y, otherMin.y, otherMax.y));
    public readonly Vector2 Clamp(Vector2 otherMin, Vector2 otherMax) =>
        new(Math.Clamp(x, otherMin.x, otherMax.x), Math.Clamp(y, otherMin.y, otherMax.y));
    public readonly Vector2Int Clamp(int otherMin, int otherMax) =>
        new(Math.Clamp(x, otherMin, otherMax), Math.Clamp(y, otherMin, otherMax));
    public readonly Vector2 Clamp(float otherMin, float otherMax) =>
        new(Math.Clamp(x, otherMin, otherMax), Math.Clamp(y, otherMin, otherMax));

    public readonly Vector2Int ComponentMin(Vector2Int other) => new(Math.Min(x, other.x), Math.Min(y, other.y));
    public readonly Vector2 ComponentMin(Vector2 other) => new(Math.Min(x, other.x), Math.Min(y, other.y));
    public static Vector2Int Min(Vector2Int a, Vector2Int b) => a.ComponentMin(b);
    public static Vector2 Min(Vector2 a, Vector2 b) => a.ComponentMin(b);

    public readonly Vector2Int ComponentMax(Vector2Int other) => new(Math.Max(x, other.x), Math.Max(y, other.y));
    public readonly Vector2 ComponentMax(Vector2 other) => new(Math.Max(x, other.x), Math.Max(y, other.y));
    public static Vector2Int Max(Vector2Int a, Vector2Int b) => a.ComponentMax(b);

    public readonly Vector2 LerpTo(Vector2 target, float frac, bool clamp = true) => Vector2.Lerp(this, target, frac, clamp);

    public static int Dot(Vector2Int a, Vector2Int b) => a.x * b.x + a.y * b.y;
    public readonly int Dot(Vector2Int b) => Dot(this, b);
    public readonly float Dot(Vector2 b) => Vector2.Dot(this, b);

    public static float DistanceBetween(Vector2Int a, Vector2 b) => (b - a).Length;
    public static float DistanceBetween(Vector2 a, Vector2Int b) => (b - a).Length;
    public readonly float Distance(Vector2 target) => DistanceBetween(this, target);

    public static float DistanceBetweenSquared(Vector2Int a, Vector2 b) => (b - a).LengthSquared;
    public static float DistanceBetweenSquared(Vector2 a, Vector2Int b) => (b - a).LengthSquared;
    public readonly float DistanceSquared(Vector2 target) => DistanceBetweenSquared(this, target);

    public readonly Vector2 Approach(float length, float amount) => Normal * Length.Approach(length, amount);

    public readonly Vector2Int Abs() => new(Math.Abs(x), Math.Abs(y));

    public static Vector2Int Reflect(Vector2Int direction, Vector2Int normal) => direction - 2 * Dot(direction, normal) * normal;

    public static Vector2Int VectorPlaneProject(Vector2Int v, Vector2Int planeNormal) => v - v.ProjectOnNormal(planeNormal);
    public readonly Vector2Int ProjectOnNormal(Vector2Int normal) => normal * Dot(this, normal);
    public readonly Vector2 ProjectOnNormal(Vector2 normal) => normal * Vector2.Dot(this, normal);

    public static void Sort(ref Vector2Int min, ref Vector2Int max)
    {
        Vector2Int vector = new(Math.Min(min.x, max.x), Math.Min(min.y, max.y));
        Vector2Int vector2 = new(Math.Max(min.x, max.x), Math.Max(min.y, max.y));
        min = vector;
        max = vector2;
    }

    public readonly bool AlmostEqual(Vector2 v, float delta = 0.0001f)
    {
        if(Math.Abs(x - v.x) > delta)
            return false;

        if(Math.Abs(y - v.y) > delta)
            return false;

        return true;
    }

    public readonly Vector2Int SubtractDirection(Vector2Int direction, int strength = 1) => this - direction * Dot(direction) * strength;
    public readonly Vector2 SubtractDirection(Vector2 direction, float strength = 1f) => this - direction * Dot(direction) * strength;
    public readonly Vector2 SnapToGrid(float gridSize, bool sx = true, bool sy = true) => ((Vector2)this).SnapToGrid(gridSize, sx, sy);


    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
    }

    public static Vector2Int Read(BinaryReader reader) => new(reader.ReadInt32(), reader.ReadInt32());

    public readonly int GetAxis(Axis axis)
    {
        if(axis == Axis.X)
            return x;
        if(axis == Axis.Y)
            return y;
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }

    public void SetAxis(Axis axis, int value)
    {
        if(axis == Axis.X)
            x = value;
        else if(axis == Axis.Y)
            y = value;
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }

    public readonly Vector2Int WithAxis(Axis axis, int value)
    {
        if(axis == Axis.X)
            return WithX(value);
        if(axis == Axis.Y)
            return WithY(value);
        throw new ArgumentException($"Given axis {axis} should be {Axis.X} or {Axis.Y}", nameof(axis));
    }

    public readonly Vector2Int WithAxes(Func<Axis, int, int> axisChanger)
    {
        return new Vector2Int(axisChanger.Invoke(Axis.X, x), axisChanger.Invoke(Axis.Y, y));
    }

    public readonly Vector2Int WithAxes(Func<int, int> axisChanger) => WithAxes((_, v) => axisChanger.Invoke(v));

    public readonly bool IsAnyAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.XY)
        {
            if(predicate.Invoke(axis, GetAxis(axis)))
                return true;
        }
        return false;
    }
    public readonly bool IsAnyAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly bool IsEveryAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.XY)
        {
            if(!predicate.Invoke(axis, GetAxis(axis)))
                return false;
        }
        return true;
    }
    public readonly bool IsEveryAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly void EachAxis(Action<Axis, int> action)
    {
        foreach(var axis in Axis.XY)
            action.Invoke(axis, GetAxis(axis));
    }
    public readonly void EachAxis(Action<int> action) => EachAxis((a, v) => action.Invoke(v));


    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.x + b.x, a.y + b.y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.x - b.x, a.y - b.y);
    public static Vector2Int operator -(Vector2Int vector) => new(-vector.x, -vector.y);
    public static Vector2Int operator *(Vector2Int vector, int value) => new(vector.x * value, vector.y * value);
    public static Vector2Int operator *(int value, Vector2Int vector) => new(vector.x * value, vector.y * value);
    public static Vector2 operator *(Vector2Int vector, float value) => new(vector.x * value, vector.y * value);
    public static Vector2 operator *(float value, Vector2Int vector) => new(vector.x * value, vector.y * value);
    public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new(a.x * b.x, a.y * b.y);
    public static Vector2 operator *(Vector2Int a, Vector2 b) => new(a.x * b.x, a.y * b.y);
    public static Vector2 operator *(Vector2 a, Vector2Int b) => new(a.x * b.x, a.y * b.y);
    public static Vector2 operator /(Vector2Int vector, float value) => new(vector.x / value, vector.y / value);
    public static Vector2Int operator /(Vector2Int a, Vector2Int b) => new(a.x / b.x, a.y / b.y);
    public static Vector2 operator /(Vector2Int a, Vector2 b) => new(a.x / b.x, a.y / b.y);
    public static Vector2 operator /(Vector2 a, Vector2Int b) => new(a.x / b.x, a.y / b.y);
    public static Vector2Int operator %(Vector2Int a, Vector2Int b) => new(a.x % b.x, a.y % b.y);


    public static implicit operator Vector2(Vector2Int vector) => new(vector.x, vector.y);
    public static implicit operator System.Numerics.Vector2(Vector2Int vector) => new(vector.x, vector.y);
    public static explicit operator Vector2Int(Vector2 vector) => new((int)vector.x, (int)vector.y);
    public static explicit operator Vector2Int(Vector3 vector) => new((int)vector.x, (int)vector.y);
    public static explicit operator Vector2Int(Vector4 vector) => new((int)vector.x, (int)vector.y);
    public static explicit operator Vector2Int(System.Numerics.Vector2 vector) => new((int)vector.X, (int)vector.Y);
    public static implicit operator Vector2Int(int all) => new(all);

    public static bool operator ==(Vector2Int left, Vector2Int right) => left.x == right.x && left.y == right.y;
    public static bool operator !=(Vector2Int left, Vector2Int right) => !(left == right);

    public override readonly bool Equals(object? obj)
    {
        if(obj is Vector2Int vector)
            return Equals(vector);
        return false;
    }
    public readonly bool Equals(Vector2Int vector) => this == vector;

    public override readonly int GetHashCode() => HashCode.Combine(x, y);


    public static Vector2Int Parse(string? str, IFormatProvider? _ = null)
    {
        if(TryParse(str, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"couldn't parse {str} to {nameof(Vector2Int)}");
    }

    public static bool TryParse([NotNullWhen(true)] string? str, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector2Int result)
    {
        result = Zero;
        if(string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        str = str.Trim('[', ']', ' ', '\n', '\r', '\t', '"');
        string[] array = str.Split(new char[5] { ' ', ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if(array.Length != 2)
        {
            return false;
        }

        if(!int.TryParse(array[0], NumberStyles.Integer, provider, out var result2) ||
            !int.TryParse(array[1], NumberStyles.Integer, provider, out var result3))
        {
            return false;
        }

        result = new Vector2Int(result2, result3);
        return true;
    }

    public static bool TryParse(string? str, out Vector2Int result) => TryParse(str, CultureInfo.InvariantCulture, out result);


    public override readonly string ToString() => ToString("0.####");

    public readonly string ToString(string? valueFormat)
    {
        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(2, 3);
        defaultInterpolatedStringHandler.AppendFormatted(x, valueFormat);
        defaultInterpolatedStringHandler.AppendLiteral(",");
        defaultInterpolatedStringHandler.AppendFormatted(y, valueFormat);
        return defaultInterpolatedStringHandler.ToStringAndClear();
    }

    public readonly BinaryTag Write()
    {
        var result = new CompoundTag();
        result.Set("x", x);
        result.Set("y", y);
        return result;
    }

    public static Vector2Int Read(BinaryTag tag)
    {
        var compoundTag = tag.To<CompoundTag>();
        return new(compoundTag.Get<int>("x"), compoundTag.Get<int>("y"));
    }

    public class Vector2IntJsonConverter : JsonConverter<Vector2Int>
    {
        public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if(reader.TokenType == JsonTokenType.String)
            {
                return Vector2Int.Parse(reader.GetString());
            }

            if(reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                Vector2Int result = default;
                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.x = reader.GetInt32();
                    reader.Read();
                }

                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.y = reader.GetInt32();
                    reader.Read();
                }

                while(reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read();
                }

                return result;
            }

            Log.Warning($"Vector2IntFromJson - unable to read from {reader.TokenType}");
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Vector2Int val, JsonSerializerOptions options)
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(2, 3);
            defaultInterpolatedStringHandler.AppendFormatted(val.x);
            defaultInterpolatedStringHandler.AppendLiteral(",");
            defaultInterpolatedStringHandler.AppendFormatted(val.y);
            writer.WriteStringValue(defaultInterpolatedStringHandler.ToStringAndClear());
        }
    }
}
