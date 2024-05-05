using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Mth;

[JsonConverter(typeof(Vector3ByteJsonConverter))]
public struct Vector3Byte : IEquatable<Vector3Byte>, IParsable<Vector3Byte>, IEnumerable<byte>, INbtWritable, INbtStaticReadable<Vector3Byte>
{
    public static readonly Vector3Byte One = new(1);
    public static readonly Vector3Byte Zero = new(0);
    public static readonly Vector3Byte Forward = (Vector3Byte)Vector3IntB.Forward;
    public static readonly Vector3Byte Left = (Vector3Byte)Vector3IntB.Left;
    public static readonly Vector3Byte Up = (Vector3Byte)Vector3IntB.Up;

    public static readonly Comparer<Vector3Byte> XYZIterationComparer = Comparer<Vector3Byte>.Create((a, b) =>
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
    public byte x { readonly get; set; }
    public byte y { readonly get; set; }
    public byte z { readonly get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public Vector3Byte(byte x, byte y, byte z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Byte(byte all)
    {
        this.x = all;
        this.y = all;
        this.z = all;
    }

    public Vector3Byte(Vector3Byte other)
    {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public static Vector3Byte FromComressed(int value)
    {
        byte x = (byte)(value & 255);
        value >>= 8;
        byte y = (byte)(value & 255);
        value >>= 8;
        byte z = (byte)(value & 255);
        return new(x, y, z);
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

    public byte this[int index]
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
                default: throw new IndexOutOfRangeException();
            }
        }
    }

    public readonly IEnumerable<Vector3Byte> GetPositionsFromZero(bool includeMaxs = true) => GetPositionsFromZero(true, includeMaxs);

    public readonly IEnumerable<Vector3Byte> GetPositionsFromZero(bool includeZero, bool includeMaxs)
    {
        var first = includeZero ? Vector3Byte.Zero : Vector3Byte.One;
        var last = includeMaxs ? this : this - 1;

        for(byte x = first.x; x <= last.x; ++x)
        {
            for(byte y = first.y; y <= last.y; ++y)
            {
                for(byte z = first.z; z <= last.z; ++z)
                {
                    yield return new(x, y, z);
                }
            }
        }
    }

    public readonly int Compress() => x | y << 8 | z << 16;

    public readonly Vector3Byte WithX(byte x) => new(x, this.y, this.z);
    public readonly Vector3Byte WithY(byte y) => new(this.x, y, this.z);
    public readonly Vector3Byte WithZ(byte z) => new(this.x, this.y, z);

    public readonly Vector3IntB WithX(int x) => new(x, this.y, this.z);
    public readonly Vector3IntB WithY(int y) => new(this.x, y, this.z);
    public readonly Vector3IntB WithZ(int z) => new(this.x, this.y, z);

    public readonly Vector3 WithX(float x) => new(x, this.y, this.z);
    public readonly Vector3 WithY(float y) => new(this.x, y, this.z);
    public readonly Vector3 WithZ(float z) => new(this.x, this.y, z);

    public readonly Vector3 ClampLength(float maxLength) => ((Vector3)this).ClampLength(maxLength);
    public readonly Vector3 ClampLength(float minLength, float maxLength) => ((Vector3)this).ClampLength(minLength, maxLength);

    public readonly Vector3Byte Clamp(Vector3IntB otherMin, Vector3IntB otherMax) =>
        new((byte)Math.Clamp(x, otherMin.x, otherMax.x), (byte)Math.Clamp(y, otherMin.y, otherMax.y), (byte)Math.Clamp(z, otherMin.z, otherMax.z));
    public readonly Vector3 Clamp(Vector3 otherMin, Vector3 otherMax) =>
        new(Math.Clamp(x, otherMin.x, otherMax.x), Math.Clamp(y, otherMin.y, otherMax.y), Math.Clamp(z, otherMin.z, otherMax.z));
    public readonly Vector3Byte Clamp(int otherMin, int otherMax) =>
        new((byte)Math.Clamp(x, otherMin, otherMax), (byte)Math.Clamp(y, otherMin, otherMax), (byte)Math.Clamp(z, otherMin, otherMax));
    public readonly Vector3 Clamp(float otherMin, float otherMax) =>
        new(Math.Clamp(x, otherMin, otherMax), Math.Clamp(y, otherMin, otherMax), Math.Clamp(z, otherMin, otherMax));

    public readonly Vector3Byte ComponentMin(Vector3Byte other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public readonly Vector3IntB ComponentMin(Vector3IntB other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public readonly Vector3 ComponentMin(Vector3 other) => new(Math.Min(x, other.x), Math.Min(y, other.y), Math.Min(z, other.z));
    public static Vector3Byte Min(Vector3Byte a, Vector3Byte b) => a.ComponentMin(b);
    public static Vector3IntB Min(Vector3IntB a, Vector3IntB b) => a.ComponentMin(b);

    public readonly Vector3Byte ComponentMax(Vector3Byte other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public readonly Vector3IntB ComponentMax(Vector3IntB other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public readonly Vector3 ComponentMax(Vector3 other) => new(Math.Max(x, other.x), Math.Max(y, other.y), Math.Max(z, other.z));
    public static Vector3Byte Max(Vector3Byte a, Vector3Byte b) => a.ComponentMax(b);
    public static Vector3IntB Max(Vector3IntB a, Vector3IntB b) => a.ComponentMax(b);

    public readonly Vector3 LerpTo(Vector3 target, float frac, bool clamp = true) => Vector3.Lerp(this, target, frac, clamp);

    public readonly Vector3IntB Cross(Vector3IntB b) => Vector3IntB.Cross(this, b);
    public readonly Vector3 Cross(Vector3 b) => Vector3.Cross(this, b);

    public readonly int Dot(Vector3IntB b) => Vector3IntB.Dot(this, b);
    public readonly float Dot(Vector3 b) => Vector3.Dot(this, b);

    public static float DistanceBetween(Vector3Byte a, Vector3 b) => (b - a).Length;
    public static float DistanceBetween(Vector3 a, Vector3Byte b) => (b - a).Length;
    public readonly float Distance(Vector3 target) => DistanceBetween(this, target);

    public static float DistanceBetweenSquared(Vector3Byte a, Vector3 b) => (b - a).LengthSquared;
    public static float DistanceBetweenSquared(Vector3 a, Vector3Byte b) => (b - a).LengthSquared;
    public readonly float DistanceSquared(Vector3 target) => DistanceBetweenSquared(this, target);

    public readonly Vector3 Approach(float length, float amount) => Normal * Length.Approach(length, amount);

    
    public readonly Vector3IntB ProjectOnNormal(Vector3IntB normal) => normal * Vector3IntB.Dot(this, normal);
    public readonly Vector3 ProjectOnNormal(Vector3 normal) => normal * Vector3.Dot(this, normal);

    public static void Sort(ref Vector3Byte min, ref Vector3Byte max)
    {
        Vector3Byte vector = new(Math.Min(min.x, max.x), Math.Min(min.y, max.y), Math.Min(min.z, max.z));
        Vector3Byte vector2 = new(Math.Max(min.x, max.x), Math.Max(min.y, max.y), Math.Max(min.z, max.z));
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

    public readonly Vector3IntB SubtractDirection(Vector3IntB direction, int strength = 1) => this - direction * Dot(direction) * strength;
    public readonly Vector3 SubtractDirection(Vector3 direction, float strength = 1f) => this - direction * Dot(direction) * strength;

    public readonly Vector3 SnapToGrid(float gridSize, bool sx = true, bool sy = true, bool sz = true) => ((Vector3)this).SnapToGrid(gridSize, sx, sy, sz);


    public readonly float Angle(Vector3 v2) => Vector3.GetAngle(this, v2);
    public static Angles VectorAngle(Vector3 vec) => Vector3.VectorAngle(vec);

    public readonly Vector3 AddClamped(Vector3 toAdd, float maxLength) => ((Vector3)this).AddClamped(toAdd, maxLength);

    public readonly Vector3 RotateAround(Vector3 center, Rotation rot) => ((Vector3)this).RotateAround(center, rot);

    public readonly Vector3 WithAcceleration(Vector3 target, float acceleration) => ((Vector3)this).WithAcceleration(target, acceleration);
    public readonly Vector3 WithFriction(float frictionAmount, float stopSpeed = 140f) => ((Vector3)this).WithFriction(frictionAmount, stopSpeed);

    public readonly byte GetAxis(Axis axis)
    {
        if(axis == Axis.X)
            return x;
        if(axis == Axis.Y)
            return y;
        return z;
    }

    public void SetAxis(Axis axis, byte value)
    {
        if(axis == Axis.X)
            x = value;
        else if(axis == Axis.Y)
            y = value;
        else
            z = value;
    }

    public readonly Vector3Byte WithAxis(Axis axis, byte value)
    {
        if(axis == Axis.X)
            return WithX(value);
        if(axis == Axis.Y)
            return WithY(value);
        return WithZ(value);
    }

    public readonly Vector3Byte WithAxes(Func<Axis, byte, byte> axisChanger)
    {
        return new Vector3Byte(axisChanger.Invoke(Axis.X, x), axisChanger.Invoke(Axis.Y, y), axisChanger.Invoke(Axis.Z, z));
    }

    public readonly Vector3Byte WithAxes(Func<byte, byte> axisChanger) => WithAxes((_, v) => axisChanger.Invoke(v));

    public readonly bool IsAnyAxis(Func<Axis, byte, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(predicate.Invoke(axis, GetAxis(axis)))
                return true;
        }
        return false;
    }
    public readonly bool IsAnyAxis(Func<byte, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly bool IsEveryAxis(Func<Axis, byte, bool> predicate)
    {
        foreach(var axis in Axis.All)
        {
            if(!predicate.Invoke(axis, GetAxis(axis)))
                return false;
        }
        return true;
    }
    public readonly bool IsEveryAxis(Func<byte, bool> predicate) => IsAnyAxis((a, v) => predicate.Invoke(v));

    public readonly void EachAxis(Action<Axis, byte> action)
    {
        foreach(var axis in Axis.All)
            action.Invoke(axis, GetAxis(axis));
    }
    public readonly void EachAxis(Action<byte> action) => EachAxis((a, v) => action.Invoke(v));


    public static Vector3IntB operator +(Vector3Byte a, Vector3Byte b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3IntB operator +(Vector3IntB a, Vector3Byte b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3IntB operator +(Vector3Byte a, Vector3IntB b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3IntB operator -(Vector3Byte a, Vector3Byte b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3IntB operator -(Vector3IntB a, Vector3Byte b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3IntB operator -(Vector3Byte a, Vector3IntB b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3IntB operator -(Vector3Byte vector) => new(-vector.x, -vector.y, -vector.z);
    public static Vector3IntB operator *(Vector3Byte vector, int value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3IntB operator *(int value, Vector3Byte vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(Vector3Byte vector, float value) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3 operator *(float value, Vector3Byte vector) => new(vector.x * value, vector.y * value, vector.z * value);
    public static Vector3IntB operator *(Vector3IntB a, Vector3Byte b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3IntB operator *(Vector3Byte a, Vector3IntB b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3IntB operator *(Vector3Byte a, Vector3Byte b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3Byte a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3 a, Vector3Byte b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
    public static Vector3 operator *(Vector3Byte vector, Rotation value) => System.Numerics.Vector3.Transform(vector, value);
    public static Vector3 operator /(Vector3Byte vector, float value) => new(vector.x / value, vector.y / value, vector.z / value);
    public static Vector3Int operator /(Vector3Byte a, Vector3IntB b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3Int operator /(Vector3Int a, Vector3Byte b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3Byte operator /(Vector3Byte a, Vector3Byte b) => new((byte)(a.x / b.x), (byte)(a.y / b.y), (byte)(a.z / b.z));
    public static Vector3 operator /(Vector3Byte a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 operator /(Vector3 a, Vector3Byte b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3Byte operator %(Vector3Byte a, Vector3Byte b) => new((byte)(a.x % b.x), (byte)(a.y % b.y), (byte)(a.z % b.z));
    public static Vector3Byte operator %(Vector3IntB a, Vector3Byte b) => new((byte)(a.x % b.x), (byte)(a.y % b.y), (byte)(a.z % b.z));
    public static Vector3Int operator %(Vector3Byte a, Vector3IntB b) => new(a.x % b.x, a.y % b.y, a.z % b.z);


    public static implicit operator Vector3IntB(Vector3Byte vector) => new(vector.x, vector.y, vector.z);
    public static implicit operator Vector3Int(Vector3Byte vector) => new(vector.x, vector.y, vector.z);
    public static explicit operator Vector3Byte(Vector3IntB vector) => new((byte)vector.x, (byte)vector.y, (byte)vector.z);
    public static explicit operator Vector3Byte(Vector3Int vector) => new((byte)vector.x, (byte)vector.y, (byte)vector.z);

    public static implicit operator Vector3(Vector3Byte vector) => new(vector.x, vector.y, vector.z);
    public static implicit operator System.Numerics.Vector3(Vector3Byte vector) => new(vector.x, vector.y, vector.z);
    public static explicit operator Vector3Byte(Vector2 vector) => new((byte)vector.x, (byte)vector.y, 0);
    public static explicit operator Vector3Byte(Vector3 vector) => new((byte)vector.x, (byte)vector.y, (byte)vector.z);
    public static explicit operator Vector3Byte(Vector4 vector) => new((byte)vector.x, (byte)vector.y, (byte)vector.z);
    public static explicit operator Vector3Byte(System.Numerics.Vector3 vector) => new((byte)vector.X, (byte)vector.Y, (byte)vector.Z);
    public static implicit operator Vector3Byte(byte all) => new(all);

    public static bool operator ==(Vector3Byte left, Vector3Byte right) => left.x == right.x && left.y == right.y && left.z == right.z;
    public static bool operator !=(Vector3Byte left, Vector3Byte right) => !(left == right);

    public override readonly bool Equals(object? obj)
    {
        if(obj is Vector3Byte vector)
            return Equals(vector);
        return false;
    }
    public readonly bool Equals(Vector3Byte vector) => this == vector;

    public override readonly int GetHashCode() => HashCode.Combine(x, y, z);


    public static Vector3Byte Parse(string? str, IFormatProvider? _ = null)
    {
        if(TryParse(str, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"couldn't parse {str} to {nameof(Vector3IntB)}");
    }

    //
    // Summary:
    //     Given a string, try to convert this into a vector. Example input formats that
    //     work would be "1,1,1", "1;1;1", "[1 1 1]". This handles a bunch of different
    //     separators ( ' ', ',', ';', '\n', '\r' ). It also trims surrounding characters
    //     ('[', ']', ' ', '\n', '\r', '\t', '"').
    public static bool TryParse([NotNullWhen(true)] string? str, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3Byte result)
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

        if(!byte.TryParse(array[0], NumberStyles.Integer, provider, out var result2) ||
            !byte.TryParse(array[1], NumberStyles.Integer, provider, out var result3) ||
            !byte.TryParse(array[2], NumberStyles.Integer, provider, out var result4))
        {
            return false;
        }

        result = new Vector3Byte(result2, result3, result4);
        return true;
    }

    public static bool TryParse(string? str, out Vector3Byte result) => TryParse(str, CultureInfo.InvariantCulture, out result);


    public override readonly string ToString() => ToString("0");

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

    public readonly IEnumerator<byte> GetEnumerator()
    {
        yield return x;
        yield return y;
        yield return z;
    }
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly BinaryTag Write() => new IntTag(Compress());

    public static Vector3Byte Read(BinaryTag tag) => Vector3Byte.FromComressed(tag.To<IntTag>().Value);

    public class Vector3ByteJsonConverter : JsonConverter<Vector3Byte>
    {
        public override Vector3Byte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if(reader.TokenType == JsonTokenType.String)
            {
                return Vector3Byte.Parse(reader.GetString());
            }

            if(reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                Vector3Byte result = default;
                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.x = reader.GetByte();
                    reader.Read();
                }

                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.y = reader.GetByte();
                    reader.Read();
                }

                if(reader.TokenType == JsonTokenType.Number)
                {
                    result.z = reader.GetByte();
                    reader.Read();
                }

                while(reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read();
                }

                return result;
            }

            Log.Warning($"ByteVector3IntFromJson - unable to read from {reader.TokenType}");
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Vector3Byte val, JsonSerializerOptions options)
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
