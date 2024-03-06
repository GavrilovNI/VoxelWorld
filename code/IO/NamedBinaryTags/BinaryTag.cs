using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values;
using System;
using System.IO;

namespace Sandcube.IO.NamedBinaryTags;

public abstract class BinaryTag : IBinaryWritable
{
    public BinaryTagType Type { get; }

    public abstract bool IsEmpty { get; }

    protected internal BinaryTag(BinaryTagType type)
    {
        Type = type;
    }

    public abstract void Write(BinaryWriter writer);


    public static BinaryTag ReadTag(BinaryReader reader)
    {
        BinaryTagType type = (BinaryTagType)reader.ReadByte();
        return ReadTag(reader, type);
    }

    public static BinaryTag ReadTag(BinaryReader reader, BinaryTagType type)
    {
        if(ValueTag.IsValueType(type))
        {
            var tag = ValueTag.Create(type);
            tag.Read(reader);
            return tag;
        }

        if(type == BinaryTagType.Compound)
            return CompoundTag.ReadTag(reader);

        if(type == BinaryTagType.List)
            return ListTag.ReadTag(reader);

        throw new NotSupportedException($"{type} is not supported");
    }
}
