using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values;
using System;
using System.IO;

namespace Sandcube.IO.NamedBinaryTags;

public abstract class BinaryTag
{
    public static readonly EmptyTag Empty = new();

    public BinaryTagType Type { get; }

    public abstract bool IsDataEmpty { get; }

    protected internal BinaryTag(BinaryTagType type)
    {
        Type = type;
    }

    public T To<T>() where T : BinaryTag, new()
    {
        if(this is T t)
            return t;
        return new T();
    }

    public abstract void WriteData(BinaryWriter writer, NbtStringPalette? palette);
    public abstract void ReadData(BinaryReader reader, NbtStringPalette? palette);


    public static void WriteType(BinaryWriter writer, BinaryTagType type) => writer.Write((byte)type);
    public static BinaryTagType ReadType(BinaryReader reader) => (BinaryTagType)reader.ReadByte();

    public void Write(BinaryWriter writer, bool usePallete = true)
    {
        writer.Write(usePallete);
        if(!usePallete)
        {
            WriteTagOnly(writer, null);
            return;
        }

        using MemoryStream memoryStream = new();
        using BinaryWriter memoryStreamWriter = new(memoryStream);

        NbtStringPalette palette = new();
        WriteTagOnly(memoryStreamWriter, palette);

        var memoryStreamBuffer = memoryStream.GetBuffer();
        var paletteTag = palette.Write();

        paletteTag.WriteTagOnly(writer, null);
        writer.Write(memoryStreamBuffer, 0, (int)memoryStream.Length);
    }

    public void WriteTagOnly(BinaryWriter writer, NbtStringPalette? palette)
    {
        BinaryTag.WriteType(writer, Type);
        WriteData(writer, palette);
    }

    public static BinaryTag Read(BinaryReader reader)
    {
        bool usePallete = reader.ReadBoolean();
        if(!usePallete)
            return ReadTagOnly(reader, null);

        var paletteTag = ReadTagOnly(reader, null);
        NbtStringPalette palette = NbtStringPalette.Read(paletteTag);
        var tag = ReadTagOnly(reader, palette);
        return tag;
    }

    public static BinaryTag ReadTagOnly(BinaryReader reader, NbtStringPalette? palette)
    {
        var type = ReadType(reader);
        return ReadTagData(reader, type, palette);
    }

    protected static BinaryTag ReadTagData(BinaryReader reader, BinaryTagType type, NbtStringPalette? palette)
    {
        var tag = CreateTag(type);
        tag.ReadData(reader, palette);
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
