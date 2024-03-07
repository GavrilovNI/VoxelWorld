using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values;
using System;
using System.IO;

namespace Sandcube.IO.NamedBinaryTags;

public abstract class BinaryTag : IBinaryWritable, IBinaryStaticReadable<BinaryTag>
{
    public static readonly EmptyTag Empty = new();

    public BinaryTagType Type { get; }

    public abstract bool IsDataEmpty { get; }

    protected internal BinaryTag(BinaryTagType type)
    {
        Type = type;
    }

    public abstract void WriteData(BinaryWriter writer);
    public abstract void ReadData(BinaryReader reader);


    public static void WriteType(BinaryWriter writer, BinaryTagType type) => writer.Write((byte)type);
    public static BinaryTagType ReadType(BinaryReader reader) => (BinaryTagType)reader.ReadByte();

    public void Write(BinaryWriter writer)
    {
        BinaryTag.WriteType(writer, Type);
        WriteData(writer);
    }

    public static BinaryTag Read(BinaryReader reader)
    {
        var type = ReadType(reader);
        return ReadTagData(reader, type);
    }

    public static T Read<T>(BinaryReader reader) where T : BinaryTag, new() =>
        Read(reader, () => new T());

    public static T Read<T>(BinaryReader reader, T defaultValue) where T : BinaryTag =>
        Read(reader, () => defaultValue);

    public static T Read<T>(BinaryReader reader, Func<T> defaultValueGetter) where T : BinaryTag
    {
        var type = ReadType(reader);
        var tag = CreateTag(type);
        if(tag is T t)
        {
            t.ReadData(reader);
            return t;
        }
        return defaultValueGetter();
    }

    public static BinaryTag ReadTagData(BinaryReader reader, BinaryTagType type)
    {
        var tag = CreateTag(type);
        tag.ReadData(reader);
        return tag;
    }

    public static BinaryTag CreateTag(BinaryTagType type)
    {
        if(type == BinaryTagType.Compound)
            return new CompoundTag();

        if(type == BinaryTagType.List)
            return new ListTag();

        if(ValueTag.IsValueType(type))
            return ValueTag.CreateValueTag(type);

        if(type == BinaryTagType.Empty)
            return BinaryTag.Empty;

        throw new NotSupportedException($"{nameof(BinaryTagType)} {type} is not supported");
    }
}
