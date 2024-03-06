using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values;
using System;
using System.IO;

namespace Sandcube.IO.NamedBinaryTags;

public abstract class BinaryTag : IBinaryWritable, IBinaryStaticReadable<BinaryTag>
{
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
        return Read(reader, type);
    }

    public static BinaryTag Read(BinaryReader reader, BinaryTagType type)
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
            return new EmptyTag();

        throw new NotSupportedException($"{nameof(BinaryTagType)} {type} is not supported");
    }
}
