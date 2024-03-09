using Sandbox;
using Sandcube.IO.NamedBinaryTags.Values;
using Sandcube.IO.NamedBinaryTags.Values.Unmanaged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandcube.IO.NamedBinaryTags.Collections;

public sealed class CompoundTag : NbtReadCollection<string>, IEnumerable<KeyValuePair<string, BinaryTag>>
{
    private readonly Dictionary<string, BinaryTag> _tags = new();

    public int Count => _tags.Count;
    public override bool IsDataEmpty => Count == 0 || _tags.Values.All(t => t.IsDataEmpty);

    public CompoundTag() : base(BinaryTagType.Compound)
    {
    }

    protected override bool TryGetTag(string key, out BinaryTag tag) => _tags.TryGetValue(key, out tag!);

    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _tags.Remove(key);
    }

    public void Clear() => _tags.Clear();

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette)
    {
        long startPosition = writer.BaseStream.Position;
        writer.Write(0L); // writing size

        writer.Write(_tags.Count);

        foreach(var (key, tag) in _tags)
        {
            palette.WriteId(writer, key);
            tag.WriteTagOnly(writer, palette);
        }

        long size = writer.BaseStream.Position - startPosition - 8;
        using(StreamPositionRememberer rememberer = writer)
        {
            writer.BaseStream.Position = startPosition;
            writer.Write(size);
        }
    }

    public override void ReadData(BinaryReader reader, NbtStringPalette? palette)
    {
        Clear();
        long _ = reader.ReadInt64(); // reading size

        int tagsCount = reader.ReadInt32();

        for(int i = 0; i < tagsCount; ++i)
        {
            string key = palette.ReadValue(reader);
            var tag = BinaryTag.ReadTagOnly(reader, palette);
            Set(key, tag);
        }
    }

    public static void Skip(BinaryReader reader)
    {
        long size = reader.ReadInt64();
        reader.BaseStream.Position += size;
    }

    public IEnumerator<KeyValuePair<string, BinaryTag>> GetEnumerator() => _tags.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Set(string key, BinaryTag tag)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if(tag is not EmptyTag)
            _tags[key] = tag;
    }

    public void Set(string key, byte value) => Set(key, new ByteTag(value));

    public void Set(string key, sbyte value) => Set(key, new SByteTag(value));

    public void Set(string key, short value) => Set(key, new ShortTag(value));

    public void Set(string key, ushort value) => Set(key, new UShortTag(value));

    public void Set(string key, int value) => Set(key, new IntTag(value));

    public void Set(string key, uint value) => Set(key, new UIntTag(value));

    public void Set(string key, long value) => Set(key, new LongTag(value));

    public void Set(string key, ulong value) => Set(key, new ULongTag(value));

    public void Set(string key, float value) => Set(key, new FloatTag(value));

    public void Set(string key, double value) => Set(key, new DoubleTag(value));

    public void Set(string key, decimal value) => Set(key, new DecimalTag(value));

    public void Set(string key, bool value) => Set(key, new BoolTag(value));

    public void Set(string key, char value) => Set(key, new CharTag(value));

    public void Set(string key, string value) => Set(key, new StringTag(value));

    public void Set<T>(string key, T value) where T : INbtWritable => Set(key, value.Write());

    public void Set<T>(string key, T value, bool _ = false) where T : struct, Enum =>
        Set(key, Array.IndexOf(Enum.GetValues<T>(), value));
}
