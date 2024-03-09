using Sandbox;
using Sandbox.UI;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using System;
using System.Text.Json.Serialization;

namespace Sandcube.Mth;
public struct RectInt : IEquatable<RectInt>, INbtWritable, INbtStaticReadable<RectInt>
{
    public int Left;

    public int Top;

    public int Right;

    public int Bottom;

    [JsonIgnore, Hide]
    public int Width
    {
        readonly get
        {
            return Right - Left;
        }
        set
        {
            Right = Left + value;
        }
    }

    [JsonIgnore, Hide]
    public int Height
    {
        readonly get
        {
            return Bottom - Top;
        }
        set
        {
            Bottom = Top + value;
        }
    }

    [JsonIgnore, Hide]
    public Vector2Int Position
    {
        readonly get
        {
            return new Vector2Int(Left, Top);
        }
        set
        {
            Vector2Int size = Size;
            Left = value.x;
            Top = value.y;
            Size = size;
        }
    }

    [JsonIgnore, Hide]
    public readonly Vector2 Center => Position + Size * 0.5f;

    public Vector2Int Size
    {
        readonly get
        {
            return new Vector2Int(Width, Height);
        }
        set
        {
            Right = Left + value.x;
            Bottom = Top + value.y;
        }
    }

    [JsonIgnore, Hide]
    public readonly RectInt WithoutPosition => new(0, 0, Width, Height);

    [JsonIgnore, Hide]
    public readonly Vector2Int BottomLeft => new(Left, Bottom);

    [JsonIgnore, Hide]
    public readonly Vector2Int BottomRight => new(Right, Bottom);

    [JsonIgnore, Hide]
    public readonly Vector2Int TopRight => new(Right, Top);

    [JsonIgnore, Hide]
    public readonly Vector2Int TopLeft => new(Left, Top);

    public RectInt(in int x, in int y, in int width, in int height)
    {
        Left = x;
        Top = y;
        Right = x + width;
        Bottom = y + height;
    }

    public RectInt(in Vector2Int position, in Vector2Int size = default) :
        this(position.x, position.y, size.x, size.y)
    {
    }

    public static RectInt FromPositionAndSize(in int x, in int y, in int width, in int height) => new()
    {
        Left = x,
        Top = y,
        Right = x + width,
        Bottom = y + height
    };

    public static RectInt FromPositionAndSize(in Vector2Int position, in Vector2Int size) => new()
    {
        Left = position.x,
        Top = position.y,
        Right = position.x + size.x,
        Bottom = position.y + size.y
    };

    public static RectInt FromSides(in int left, in int top, in int right, in int bottom) => new()
    {
        Left = left,
        Top = top,
        Right = right,
        Bottom = bottom
    };

    public readonly void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = Left;
        y = Top;
        width = Width;
        height = Height;
    }

    public readonly void Deconstruct(out Vector2Int position, out Vector2Int size)
    {
        position = Position;
        size = Size;
    }


    public readonly bool IsInside(in RectInt rect, bool fullyInside = false)
    {
        if(fullyInside)
            return Left < rect.Left && Right > rect.Right && Top < rect.Top && Bottom > rect.Bottom;

        if(rect.Right < Left)
            return false;

        if(rect.Left > Right)
            return false;

        if(rect.Top > Bottom)
            return false;

        if(rect.Bottom < Top)
            return false;

        return true;
    }

    public readonly bool IsInside(in Rect rect, bool fullyInside = false)
    {
        if(fullyInside)
            return Left < rect.Left && Right > rect.Right && Top < rect.Top && Bottom > rect.Bottom;

        if(rect.Right < Left)
            return false;

        if(rect.Left > Right)
            return false;

        if(rect.Top > Bottom)
            return false;

        if(rect.Bottom < Top)
            return false;

        return true;
    }

    public readonly bool IsInside(in Vector2Int pos)
    {
        if(pos.x < Left)
            return false;

        if(pos.x > Right)
            return false;

        if(pos.y > Bottom)
            return false;

        if(pos.y < Top)
            return false;

        return true;
    }

    public readonly bool IsInside(in Vector2 pos)
    {
        if(pos.x < Left)
            return false;

        if(pos.x > Right)
            return false;

        if(pos.y > Bottom)
            return false;

        if(pos.y < Top)
            return false;

        return true;
    }

    //
    // Summary:
    //     Returns a Rect shrunk in every direction by given values.

    public readonly RectInt Shrink(in int left, in int top, in int right, in int bottom)
    {
        RectInt result = this;
        result.Left += left;
        result.Top += top;
        result.Right -= right;
        result.Bottom -= bottom;
        return result;
    }

    public readonly Rect Shrink(in float left, in float top, in float right, in float bottom)
    {
        Rect result = this;
        result.Left += left;
        result.Top += top;
        result.Right -= right;
        result.Bottom -= bottom;
        return result;
    }

    //
    // Summary:
    //     Returns a Rect shrunk in every direction by Margin's values.
    public readonly Rect Shrink(in Margin m)
    {
        float num = m.Left;
        float num2 = m.Top;
        float num3 = m.Right;
        float num4 = m.Bottom;
        return Shrink(in num, in num2, in num3, in num4);
    }

    //
    // Summary:
    //     Returns a Rect shrunk in every direction by given values on each axis.
    public readonly RectInt Shrink(in int x, in int y) => Shrink(in x, in y, in x, in y);
    public readonly Rect Shrink(in float x, in float y) => Shrink(in x, in y, in x, in y);

    //
    // Summary:
    //     Returns a Rect shrunk in every direction by given amount.
    public readonly RectInt Shrink(in int amt) => Shrink(in amt, in amt, in amt, in amt);
    public readonly Rect Shrink(in float amt) => Shrink(in amt, in amt, in amt, in amt);

    public readonly RectInt Grow(in int left, in int top, in int right, in int bottom)
    {
        RectInt result = this;
        result.Left -= left;
        result.Top -= top;
        result.Right += right;
        result.Bottom += bottom;
        return result;
    }
    public readonly Rect Grow(in float left, in float top, in float right, in float bottom)
    {
        Rect result = this;
        result.Left -= left;
        result.Top -= top;
        result.Right += right;
        result.Bottom += bottom;
        return result;
    }
    public readonly Rect Grow(Margin m)
    {
        float num = m.Left;
        float num2 = m.Top;
        float num3 = m.Right;
        float num4 = m.Bottom;
        return Grow(in num, in num2, in num3, in num4);
    }
    public readonly RectInt Grow(in int x, in int y) => Grow(in x, in y, in x, in y);
    public readonly Rect Grow(in float x, in float y) => Grow(in x, in y, in x, in y);
    public readonly RectInt Grow(in int amt) => Grow(in amt, in amt, in amt, in amt);
    public readonly Rect Grow(in float amt) => Grow(in amt, in amt, in amt, in amt);

    public static implicit operator Rect(RectInt rect) => new(rect.Left, rect.Top, rect.Width, rect.Height);
    public static explicit operator RectInt(Rect rect) => new()
    {
        Left = (int)rect.Left,
        Top = (int)rect.Top,
        Right = (int)rect.Right,
        Bottom = (int)rect.Bottom
    };

    public static RectInt operator +(in RectInt a, in RectInt b) => new(a.Left + b.Left, a.Top + b.Top, a.Width + b.Width, a.Height + b.Height);
    public static Rect operator +(in RectInt a, in Rect b) => new(a.Left + b.Left, a.Top + b.Top, a.Width + b.Width, a.Height + b.Height);
    public static Rect operator +(in Rect a, in RectInt b) => b + a;

    public static RectInt operator *(in RectInt a, in int b) => new(a.Left * b, a.Top * b, a.Width * b, a.Height * b);
    public static Rect operator *(in RectInt a, in float b) => new(a.Left * b, a.Top * b, a.Width * b, a.Height * b);

    public static RectInt operator *(in RectInt a, in Vector2Int b) => new(a.Left * b.x, a.Top * b.y, a.Width * b.x, a.Height * b.y);
    public static Rect operator *(in RectInt a, in Vector2 b) => new(a.Left * b.x, a.Top * b.y, a.Width * b.x, a.Height * b.y);

    public override readonly string ToString() => $"{Left},{Top},{Width},{Height}";

    public readonly Vector4 ToVector4() => new (Left, Top, Right, Bottom);

    public void Add(RectInt r)
    {
        Left = Math.Min(Left, r.Left);
        Right = Math.Max(Right, r.Right);
        Top = Math.Min(Top, r.Top);
        Bottom = Math.Max(Bottom, r.Bottom);
    }

    public void Add(Vector2Int point)
    {
        Left = Math.Min(Left, point.x);
        Right = Math.Max(Right, point.x);
        Top = Math.Min(Top, point.y);
        Bottom = Math.Max(Bottom, point.y);
    }

    public readonly RectInt AddPoint(Vector2Int pos)
    {
        RectInt result = this;
        result.Add(pos);
        return result;
    }

    public readonly Rect AddPoint(Vector2 pos)
    {
        Rect result = this;
        result.Add(pos);
        return result;
    }

    public readonly Rect Align(in Vector2 size, TextFlag align)
    {
        Vector2 point = Position;
        Rect result = new(in point, in size);
        if(align.HasFlag(TextFlag.Right))
        {
            result.Left = Right - size.x;
            result.Right = Right;
        }

        if(align.HasFlag(TextFlag.CenterHorizontally))
        {
            result.Left = Left + (Width - size.x) * 0.5f;
            result.Right = result.Left + size.x;
        }

        if(align.HasFlag(TextFlag.Bottom))
        {
            result.Top = Bottom - size.y;
            result.Bottom = Bottom;
        }

        if(align.HasFlag(TextFlag.CenterVertically))
        {
            result.Top = Top + (Height - size.y) * 0.5f;
            result.Bottom = result.Top + size.y;
        }

        return result;
    }

    public readonly Rect Contain(in Vector2 size, TextFlag align = TextFlag.Center, bool stretch = false)
    {
        float num = Math.Min(Width / size.x, Height / size.y);
        if(!stretch)
        {
            num = Math.Min(num, 1f);
        }

        Vector2 vector = default;
        vector.x = MathF.Ceiling(size.x * num);
        vector.y = MathF.Ceiling(size.y * num);
        Vector2 size2 = vector;
        return Align(in size2, align);
    }

    public static RectInt operator +(RectInt r, in Vector2Int p) => new(r.Position + p, r.Size);
    public static Rect operator +(RectInt r, in Vector2 p) => new(r.Position + p, r.Size);


    public static bool operator ==(RectInt left, RectInt right) => left.Equals(right);
    public static bool operator !=(RectInt left, RectInt right) => !(left == right);


    public readonly override bool Equals(object? obj) => obj is RectInt o && Equals(o);
    public readonly bool Equals(RectInt o) => Left == o.Left && Right == o.Right && Top == o.Top && Bottom == o.Bottom;

    public override readonly int GetHashCode() => HashCode.Combine(Left, Right, Top, Bottom);

    public readonly BinaryTag Write() => new ListTag
    {
        Left,
        Top,
        Width,
        Height
    };

    public static RectInt Read(BinaryTag tag)
    {
        var listTag = tag.To<ListTag>();
        return new(listTag.Get<int>(0), listTag.Get<int>(1), listTag.Get<int>(2), listTag.Get<int>(3));
    }
}
