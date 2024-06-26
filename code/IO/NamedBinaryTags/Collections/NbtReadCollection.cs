﻿using VoxelWorld.IO.NamedBinaryTags.Values;
using VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;
using System;
using VoxelWorld.Mth;

namespace VoxelWorld.IO.NamedBinaryTags.Collections;

public abstract class NbtReadCollection<TKey> : BinaryTag
{
    protected internal NbtReadCollection(BinaryTagType type) : base(type)
    {
    }

    protected abstract bool TryGetTag(TKey key, out BinaryTag tag);

    
    public bool HasTag(TKey key) => TryGetTag(key, out _);

    public bool HasTag<T>(TKey key) where T : BinaryTag
    {
        if(!TryGetTag(key, out var tag))
            return false;
        return tag is T;
    }

    public BinaryTag GetTag(TKey key)
    {
        if(TryGetTag(key, out var tag))
            return tag;
        return BinaryTag.Empty;
    }

    public BinaryTag? GetTagOrNull(TKey key)
    {
        if(TryGetTag(key, out var tag))
            return tag;
        return null;
    }

    public T GetTag<T>(TKey key) where T : BinaryTag, new()
    {
        if(TryGetTag(key, out var tag))
            return tag as T ?? new T();
        return new T();
    }

    public T GetTag<T>(TKey key, T defaultTag) where T : BinaryTag
    {
        if(TryGetTag(key, out var tag))
            return tag as T ?? defaultTag;
        return defaultTag;
    }

    public T GetTag<T>(TKey key, Func<T> defaultTagSupplier) where T : BinaryTag
    {
        if(TryGetTag(key, out var tag))
            return tag as T ?? defaultTagSupplier();
        return defaultTagSupplier();
    }

    public byte Get<T>(TKey key, byte defaultValue = default) where T : IEquatable<byte> =>
        Get<T>(key, defaultValue, false)!.Value;

    public byte? Get<T>(TKey key, byte? defaultValue, bool _ = false) where T : IEquatable<byte>
    {
        if(TryGetTag(key, out var tag) && tag is ByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public sbyte Get<T>(TKey key, sbyte defaultValue = default) where T : IEquatable<sbyte> =>
        Get<T>(key, defaultValue, false)!.Value;

    public sbyte? Get<T>(TKey key, sbyte? defaultValue, bool _ = false) where T : IEquatable<sbyte>
    {
        if(TryGetTag(key, out var tag) && tag is SByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public short Get<T>(TKey key, short defaultValue = default) where T : IEquatable<short> =>
        Get<T>(key, defaultValue, false)!.Value;

    public short? Get<T>(TKey key, short? defaultValue, bool _ = false) where T : IEquatable<short>
    {
        if(TryGetTag(key, out var tag) && tag is SByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public ushort Get<T>(TKey key, ushort defaultValue = default) where T : IEquatable<ushort> =>
        Get<T>(key, defaultValue, false)!.Value;

    public ushort? Get<T>(TKey key, ushort? defaultValue, bool _ = false) where T : IEquatable<ushort>
    {
        if(TryGetTag(key, out var tag) && tag is UShortTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public int Get<T>(TKey key, int defaultValue = default) where T : IEquatable<int> =>
        Get<T>(key, defaultValue, false)!.Value;

    public int? Get<T>(TKey key, int? defaultValue, bool _ = false) where T : IEquatable<int>
    {
        if(TryGetTag(key, out var tag) && tag is IntTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public uint Get<T>(TKey key, uint defaultValue = default) where T : IEquatable<uint> =>
        Get<T>(key, defaultValue, false)!.Value;

    public uint? Get<T>(TKey key, uint? defaultValue, bool _ = false) where T : IEquatable<uint>
    {
        if(TryGetTag(key, out var tag) && tag is UIntTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public long Get<T>(TKey key, long defaultValue = default) where T : IEquatable<long> =>
        Get<T>(key, defaultValue, false)!.Value;

    public long? Get<T>(TKey key, long? defaultValue, bool _ = false) where T : IEquatable<long>
    {
        if(TryGetTag(key, out var tag) && tag is LongTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public ulong Get<T>(TKey key, ulong defaultValue = default) where T : IEquatable<ulong> =>
        Get<T>(key, defaultValue, false)!.Value;

    public ulong? Get<T>(TKey key, ulong? defaultValue, bool _ = false) where T : IEquatable<ulong>
    {
        if(TryGetTag(key, out var tag) && tag is ULongTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public float Get<T>(TKey key, float defaultValue = default) where T : IEquatable<float> =>
        Get<T>(key, defaultValue, false)!.Value;

    public float? Get<T>(TKey key, float? defaultValue, bool _ = false) where T : IEquatable<float>
    {
        if(TryGetTag(key, out var tag) && tag is FloatTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public double Get<T>(TKey key, double defaultValue = default) where T : IEquatable<double> =>
        Get<T>(key, defaultValue, false)!.Value;

    public double? Get<T>(TKey key, double? defaultValue, bool _ = false) where T : IEquatable<double>
    {
        if(TryGetTag(key, out var tag) && tag is DoubleTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public decimal Get<T>(TKey key, decimal defaultValue = default) where T : IEquatable<decimal> =>
        Get<T>(key, defaultValue, false)!.Value;

    public decimal? Get<T>(TKey key, decimal? defaultValue, bool _ = false) where T : IEquatable<decimal>
    {
        if(TryGetTag(key, out var tag) && tag is DecimalTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public bool Get<T>(TKey key, bool defaultValue = default) where T : IEquatable<bool> =>
        Get<T>(key, defaultValue, false)!.Value;

    public bool? Get<T>(TKey key, bool? defaultValue, bool _ = false) where T : IEquatable<bool>
    {
        if(TryGetTag(key, out var tag) && tag is BoolTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public char Get<T>(TKey key, char defaultValue = default) where T : IEquatable<char> =>
        Get<T>(key, defaultValue, false)!.Value;

    public char? Get<T>(TKey key, char? defaultValue, bool _ = false) where T : IEquatable<char>
    {
        if(TryGetTag(key, out var tag) && tag is CharTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public string Get<T>(TKey key, string defaultValue = "") where T : IEquatable<string> =>
        Get<T>(key, defaultValue, false)!;

    public string? Get<T>(TKey key, string? defaultValue, bool _ = false) where T : IEquatable<string>
    {
        if(TryGetTag(key, out var tag) && tag is StringTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public T Get<T>(TKey key, T defaultValue) where T : struct, Enum =>
        Get<T>(key, defaultValue, false)!.Value;

    public T? Get<T>(TKey key, T? defaultValue, bool _ = false) where T : struct, Enum
    {
        if(TryGetTag(key, out var tag) && tag is IntTag intTag)
            return Enum.GetValues<T>()[intTag.Value];
        return defaultValue;
    }

    // TODO: uncomment/implement when get whitelisted
    //public T? Get<T>(TKey key, T? defaultValue = default) where T : INbtStaticReadable<T>
    //{
    //    if(TryGetTag(key, out var tag))
    //        return T.Read(tag);
    //    return defaultValue;
    //}

    public void ReadBy<T>(TKey key, ref T value) where T : INbtReadable
    {
        var tag = GetTag(key);
        value.Read(tag);
    }

    public bool TryReadBy<T>(TKey key, ref T value) where T : INbtReadable
    {
        if(!TryGetTag(key, out var tag))
            return false;
        value.Read(tag);
        return true;
    }
}
