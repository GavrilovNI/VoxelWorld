using Sandbox;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandcube.Mth;

[JsonConverter(typeof(Vector3IntJsonConverter))]
public struct Vector3Int : IEquatable<Vector3Int>, IParsable<Vector3Int>, INbtWritable, INbtStaticReadable<Vector3Int>
{
    public static readonly Vector3Int One = new(1);
    public static readonly Vector3Int Zero = new(0);
    public static readonly Vector3Int Forward = (Vector3Int)Vector3.Forward;
    public static readonly Vector3Int Backward = (Vector3Int)Vector3.Backward;
    public static readonly Vector3Int Left = (Vector3Int)Vector3.Left;
    public static readonly Vector3Int Right = (Vector3Int)Vector3.Right;
    public static readonly Vector3Int Up = (Vector3Int)Vector3.Up;
    public static readonly Vector3Int Down = (Vector3Int)Vector3.Down;

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

    public Vector3Int(Vector2Int vector, int z = 0)
    {
        this.x = vector.x;
        this.y = vector.y;
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


    [JsonIgnore]
    public readonly Vector3 Normal
    {
        get
        {
            if(this == Zero)
                return this;

            return this / Length;
        }
    }

    [JsonIgnore]
    public readonly float Length => MathF.Sqrt(LengthSquared);

    [JsonIgnore]
    public readonly float LengthSquared => x * x + y * y + z * z;

    public readonly Angles EulerAngles => VectorAngle(this);

    public readonly Vector3 Inverse => new(1f / x, 1f / y, 1f / z);

    public int this[int index]
    {
        readonly get
        {
            return index switch
            {
                0 => x,
                1 => y,
                2 => z,
                _ => throw new IndexOutOfRangeException(),
            };
        }
        set
        {
            switch(index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
            }
        }
    }

    public readonly IEnumerable<Vector3Int> GetPositionsFromZero(bool includeMaxs = true) => GetPositionsFromZero(true, includeMaxs);

    public readonly IEnumerable<Vector3Int> GetPositionsFromZero(bool includeZero, bool includeMaxs)
    {
        var first = includeZero ? Vector3Int.Zero : Vector3Int.One;
        var last = includeMaxs ? this : this - 1;

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
    public readonly Vector3Int Clamp(int otherMin, int otherMax) =>
        new(Math.Clamp(x, otherMin, otherMax), Math.Clamp(y, otherMin, otherMax), Math.Clamp(z, otherMin, otherMax));
    public readonly Vector3 Clamp(float otherMin, float otherMax) =>
        new(Math.Clamp(x, otherMin, otherMax), Math.Clamp(y, otherMin, otherMax), Math.Clamp(z, otherMin, otherMax));

    public readonly Vector3Int ComponentMin(Vector3Int other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public readonly Vector3 ComponentMin(Vector3 other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public static Vector3Int Min(Vector3Int a, Vector3Int b) => a.ComponentMin(b);

    public readonly Vector3Int ComponentMax(Vector3Int other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public readonly Vector3 ComponentMax(Vector3 other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public static Vector3Int Max(Vector3Int a, Vector3Int b) => a.ComponentMax(b);

    public readonly Vector3 LerpTo(Vector3 target, float frac, bool clamp = true) => Vector3.Lerp(this, target, frac, clamp);

    public static Vector3Int Cross(Vector3Int a, Vector3Int b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    public readonly Vector3Int Cross(Vector3Int b) => Cross(this, b);
    public readonly Vector3 Cross(Vector3 b) => Vector3.Cross(this, b);

    public static int Dot(Vector3Int a, Vector3Int b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public readonly int Dot(Vector3Int b) => Dot(this, b);
    public readonly float Dot(Vector3 b) => Vector3.Dot(this, b);

    public static float DistanceBetween(Vector3Int a, Vector3 b) => (b - a).Length;
    public static float DistanceBetween(Vector3 a, Vector3Int b) => (b - a).Length;
    public readonly float Distance(Vector3 target) => DistanceBetween(this, target);

    public static float DistanceBetweenSquared(Vector3Int a, Vector3 b) => (b - a).LengthSquared;
    public static float DistanceBetweenSquared(Vector3 a, Vector3Int b) => (b - a).LengthSquared;
    public readonly float DistanceSquared(Vector3 target) => DistanceBetweenSquared(this, target);

    public readonly Vector3 Approach(float length, float amount) => Normal * Length.Approach(length, amount);

    public readonly Vector3Int Abs() => new(Math.Abs(x), Math.Abs(y), Math.Abs(z));

    public static Vector3Int Reflect(Vector3Int direction, Vector3Int normal) => direction - 2 * Dot(direction, normal) * normal;

    public static Vector3Int VectorPlaneProject(Vector3Int v, Vector3Int planeNormal) => v - v.ProjectOnNormal(planeNormal);
    public readonly Vector3Int ProjectOnNormal(Vector3Int normal) => normal * Dot(this, normal);
    public readonly Vector3 ProjectOnNormal(Vector3 normal) => normal * Vector3.Dot(this, normal);

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

    public static Vector3 CubicBezier(Vector3 source, Vector3 target, Vector3 sourceTangent, Vector3 targetTangent, float t) =>
        Vector3.CubicBezier(source, target, sourceTangent, targetTangent, t);

    public readonly Vector3Int SubtractDirection(Vector3Int direction, int strength = 1) => this - direction * Dot(direction) * strength;
    public readonly Vector3 SubtractDirection(Vector3 direction, float strength = 1f) => this - direction * Dot(direction) * strength;

    public readonly Vector3 SnapToGrid(float gridSize, bool sx = true, bool sy = true, bool sz = true) => ((Vector3)this).SnapToGrid(gridSize, sx, sy, sz);

    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float deltaTime) =>
        Vector3.SmoothDamp(current, target, ref velocity, smoothTime, deltaTime);

    public static float GetAngle(Vector3 v1, Vector3 v2) => Vector3.GetAngle(v1, v2);
    public readonly float Angle(Vector3 v2) => GetAngle(this, v2);
    public static Angles VectorAngle(Vector3 vec) => Vector3.VectorAngle(vec);

    public readonly Vector3 AddClamped(Vector3 toAdd, float maxLength) => ((Vector3)this).AddClamped(toAdd, maxLength);

    public readonly Vector3 RotateAround(Vector3 center, Rotation rot) => ((Vector3)this).RotateAround(center, rot);

    public readonly Vector3 WithAcceleration(Vector3 target, float acceleration) => ((Vector3)this).WithAcceleration(target, acceleration);
    public readonly Vector3 WithFriction(float frictionAmount, float stopSpeed = 140f) => ((Vector3)this).WithFriction(frictionAmount, stopSpeed);

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

    public readonly bool IsAnyAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, GetAxis(axis)))
                return true;
        }
        return false;
    }
    public readonly bool IsAnyAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly bool IsEveryAxis(Func<Axis, int, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, GetAxis(axis)))
                return false;
        }
        return true;
    }
    public readonly bool IsEveryAxis(Func<int, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly void EachAxis(Action<Axis, int> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, GetAxis(axis));
    }
    public readonly void EachAxis(Action<int> action) => EachAxis((a, v) => action.Invoke(v));


    public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3Int operator -(Vector3Int vector) => new(-vector.x, -vector.y, -vector.z);
    public static Vector3Int operator *(Vector3Int vector, int value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3Int operator *(int value, Vector3Int vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(Vector3Int vector, float value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(float value, Vector3Int vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3Int operator *(Vector3Int a, Vector3Int b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3Int a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3 a, Vector3Int b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3Int vector, Rotation value) => System.Numerics.Vector3.Transform(vector, value);
    public static Vector3 operator /(Vector3Int vector, float value) => new(vector.x / value, vector.y / value, vector.z / value);
    public static Vector3Int operator /(Vector3Int a, Vector3Int b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 operator /(Vector3Int a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 operator /(Vector3 a, Vector3Int b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3Int operator %(Vector3Int a, Vector3Int b) => new(a.x % b.x, a.y % b.y, a.z % b.z);


    public static implicit operator Vector3(Vector3Int vector) => new(vector.x, vector.y, vector.z);
    public static implicit operator System.Numerics.Vector3(Vector3Int vector) => new(vector.x, vector.y, vector.z);
    public static explicit operator Vector3Int(Vector2 vector) => new((int)vector.x, (int)vector.y, 0);
    public static explicit operator Vector3Int(Vector3 vector) => new((int)vector.x, (int)vector.y, (int)vector.z);
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
    public readonly bool Equals(Vector3Int vector) => this == vector;

    public override readonly int GetHashCode() => HashCode.Combine(x, y, z);


    public static Vector3Int Parse(string? str, IFormatProvider? _ = null)
    {
        if(TryParse(str, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"couldn't parse {str} to {nameof(Vector3Int)}");
    }

    //
    // Summary:
    //     Given a string, try to convert this into a vector. Example input formats that
    //     work would be "1,1,1", "1;1;1", "[1 1 1]". This handles a bunch of different
    //     separators ( ' ', ',', ';', '\n', '\r' ). It also trims surrounding characters
    //     ('[', ']', ' ', '\n', '\r', '\t', '"').
    public static bool TryParse([NotNullWhen(true)] string? str, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3Int result)
    {
        result = Zero;
        if(string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        str = str.Trim('[', ']', ' ', '\n', '\r', '\t', '"');
        string[] array = str.Split(new char[5] { ' ', ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if(array.Length != 3)
        {
            return false;
        }

        if(!int.TryParse(array[0], NumberStyles.Integer, provider, out var result2) ||
            !int.TryParse(array[1], NumberStyles.Integer, provider, out var result3) ||
            !int.TryParse(array[2], NumberStyles.Integer, provider, out var result4))
        {
            return false;
        }

        result = new Vector3Int(result2, result3, result4);
        return true;
    }

    public static bool TryParse(string? str, out Vector3Int result) => TryParse(str, CultureInfo.InvariantCulture, out result);


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

    public readonly BinaryTag Write() => new ListTag
    {
        x,
        y,
        z
    };

    public static Vector3Int Read(BinaryTag tag)
    {
        var listTag = tag.To<ListTag>();
        return new(listTag.Get<int>(0), listTag.Get<int>(1), listTag.Get<int>(2));
    }

    public class Vector3IntJsonConverter : JsonConverter<Vector3Int>
    {
        public override Vector3Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if(reader.TokenType == JsonTokenType.String)
            {
                return Vector3Int.Parse(reader.GetString());
            }

            if(reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                Vector3Int result = default;
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

                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.z = reader.GetInt32();
                    reader.Read();
                }

                while(reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read();
                }

                return result;
            }

            Log.Warning($"Vector3IntFromJson - unable to read from {reader.TokenType}");
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Vector3Int val, JsonSerializerOptions options)
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(2, 3);
            defaultInterpolatedStringHandler.AppendFormatted(val.x);
            defaultInterpolatedStringHandler.AppendLiteral(",");
            defaultInterpolatedStringHandler.AppendFormatted(val.y);
            defaultInterpolatedStringHandler.AppendLiteral(",");
            defaultInterpolatedStringHandler.AppendFormatted(val.z);
            writer.WriteStringValue(defaultInterpolatedStringHandler.ToStringAndClear());
        }
    }
}
