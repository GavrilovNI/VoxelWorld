using Sandcube.IO.NamedBinaryTags.Values;
using Sandcube.IO.NamedBinaryTags.Values.Unmanaged;
using System;

namespace Sandcube.IO.NamedBinaryTags.Collections;

public abstract class NbtReadCollection<TKey> : BinaryTag
{
    protected internal NbtReadCollection(BinaryTagType type) : base(type)
    {
    }

    public abstract bool HasTag(TKey key);
    public abstract BinaryTag GetTag(TKey key);

    public bool TryGet(TKey key, out BinaryTag tag)
    {
        if(!HasTag(key))
        {
            tag = null!;
            return false;
        }
        tag = GetTag(key);
        return true;
    }

    public BinaryTag GetTagOrDefault(TKey key, BinaryTag defaultTag)
    {
        if(!HasTag(key))
            return defaultTag;
        return GetTag(key);
    }

    public BinaryTag GetTagOrDefault(TKey key, Func<BinaryTag> defaultTagGetter)
    {
        if(!HasTag(key))
            return defaultTagGetter();
        return GetTag(key);
    }


    public byte Get<T>(TKey key, byte defaultValue = default) where T : IEquatable<byte>
    {
        if(TryGet(key, out var tag) && tag is ByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public sbyte Get<T>(TKey key, sbyte defaultValue = default) where T : IEquatable<sbyte>
    {
        if(TryGet(key, out var tag) && tag is SByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public short Get<T>(TKey key, short defaultValue = default) where T : IEquatable<short>
    {
        if(TryGet(key, out var tag) && tag is SByteTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public ushort Get<T>(TKey key, ushort defaultValue = default) where T : IEquatable<ushort>
    {
        if(TryGet(key, out var tag) && tag is UShortTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public int Get<T>(TKey key, int defaultValue = default) where T : IEquatable<int>
    {
        if(TryGet(key, out var tag) && tag is IntTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public uint Get<T>(TKey key, uint defaultValue = default) where T : IEquatable<uint>
    {
        if(TryGet(key, out var tag) && tag is UIntTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public long Get<T>(TKey key, long defaultValue = default) where T : IEquatable<long>
    {
        if(TryGet(key, out var tag) && tag is LongTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public ulong Get<T>(TKey key, ulong defaultValue = default) where T : IEquatable<ulong>
    {
        if(TryGet(key, out var tag) && tag is ULongTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public float Get<T>(TKey key, float defaultValue = default) where T : IEquatable<float>
    {
        if(TryGet(key, out var tag) && tag is FloatTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public double Get<T>(TKey key, double defaultValue = default) where T : IEquatable<double>
    {
        if(TryGet(key, out var tag) && tag is DoubleTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public decimal Get<T>(TKey key, decimal defaultValue = default) where T : IEquatable<decimal>
    {
        if(TryGet(key, out var tag) && tag is DecimalTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public bool Get<T>(TKey key, bool defaultValue = default) where T : IEquatable<bool>
    {
        if(TryGet(key, out var tag) && tag is BoolTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public char Get<T>(TKey key, char defaultValue = default) where T : IEquatable<char>
    {
        if(TryGet(key, out var tag) && tag is CharTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public string Get<T>(TKey key, string defaultValue = "") where T : IEquatable<string>
    {
        if(TryGet(key, out var tag) && tag is StringTag realTag)
            return realTag.Value;
        return defaultValue;
    }

    public T Get<T>(TKey key, T defaultValue) where T : struct, Enum
    {
        if(TryGet(key, out var tag) && tag is IntTag intTag)
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
        if(!TryGet(key, out var tag))
            return false;
        value.Read(tag);
        return true;
    }
}
