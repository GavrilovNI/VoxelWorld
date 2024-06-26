﻿using VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values;

public abstract class ValueTag : BinaryTag
{
    protected internal ValueTag(BinaryTagType type) : base(type)
    {
    }

    public static bool IsValueType(BinaryTagType type)
    {
        return type switch
        {
            BinaryTagType.Byte or
            BinaryTagType.SByte or
            BinaryTagType.Short or
            BinaryTagType.UShort or
            BinaryTagType.Int or
            BinaryTagType.UInt or
            BinaryTagType.Long or
            BinaryTagType.ULong or
            BinaryTagType.Float or
            BinaryTagType.Double or
            BinaryTagType.Decimal or
            BinaryTagType.Bool or
            BinaryTagType.Char or
            BinaryTagType.String => true,
            _ => false,
        };
    }
    
    public static ValueTag CreateValueTag(BinaryTagType type)
    {
        return type switch
        {
            BinaryTagType.Byte => new ByteTag(),
            BinaryTagType.SByte => new SByteTag(),
            BinaryTagType.Short => new ShortTag(),
            BinaryTagType.UShort => new UShortTag(),
            BinaryTagType.Int => new IntTag(),
            BinaryTagType.UInt => new UIntTag(),
            BinaryTagType.Long => new LongTag(),
            BinaryTagType.ULong => new ULongTag(),
            BinaryTagType.Float => new FloatTag(),
            BinaryTagType.Double => new DoubleTag(),
            BinaryTagType.Decimal => new DecimalTag(),
            BinaryTagType.Bool => new BoolTag(),
            BinaryTagType.Char => new CharTag(),
            BinaryTagType.String => new StringTag(),
            _ => throw new ArgumentException($"{type} is not a value type")
        };
    }
}

public abstract class ValueTag<T> : ValueTag, IEquatable<T>
{
    public virtual T Value { get; set; } 

    protected internal ValueTag(BinaryTagType type, T value) : base(type)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    public abstract bool Equals(T? other);
}
