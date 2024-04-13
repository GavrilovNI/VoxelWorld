using VoxelWorld.IO.NamedBinaryTags.Values;
using VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VoxelWorld.IO.NamedBinaryTags.Collections;

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
        var tagsToWrite = _tags.Where(t => !t.Value.IsDataEmpty);
        writer.Write(tagsToWrite.Count());

        foreach(var (key, tag) in tagsToWrite)
        {
            palette.WriteId(writer, key);
            tag.WriteTagOnly(writer, palette);
        }
    }

    public override void ReadData(BinaryReader reader, NbtStringPalette? palette)
    {
        Clear();

        int tagsCount = reader.ReadInt32();

        for(int i = 0; i < tagsCount; ++i)
        {
            string key = palette.ReadValue(reader);
            var tag = BinaryTag.ReadTagOnly(reader, palette);
            Set(key, tag);
        }
    }

    public IEnumerator<KeyValuePair<string, BinaryTag>> GetEnumerator() => _tags.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Set(string key, BinaryTag? tag)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if(tag is null)
            _tags.Remove(key);
        else if(tag is not EmptyTag)
            _tags[key] = tag;
    }

    public void Set(string key, byte? value) => Set(key, value is null ? null : new ByteTag(value.Value));

    public void Set(string key, sbyte? value) => Set(key, value is null ? null : new SByteTag(value.Value));

    public void Set(string key, short? value) => Set(key, value is null ? null : new ShortTag(value.Value));

    public void Set(string key, ushort? value) => Set(key, value is null ? null : new UShortTag(value.Value));

    public void Set(string key, int? value) => Set(key, value is null ? null : new IntTag(value.Value));

    public void Set(string key, uint? value) => Set(key, value is null ? null : new UIntTag(value.Value));

    public void Set(string key, long? value) => Set(key, value is null ? null : new LongTag(value.Value));

    public void Set(string key, ulong? value) => Set(key, value is null ? null : new ULongTag(value.Value));

    public void Set(string key, float? value) => Set(key, value is null ? null : new FloatTag(value.Value));

    public void Set(string key, double? value) => Set(key, value is null ? null : new DoubleTag(value.Value));

    public void Set(string key, decimal? value) => Set(key, value is null ? null : new DecimalTag(value.Value));

    public void Set(string key, bool? value) => Set(key, value is null ? null : new BoolTag(value.Value));

    public void Set(string key, char? value) => Set(key, value is null ? null : new CharTag(value.Value));

    public void Set(string key, string? value) => Set(key, value is null ? null : new StringTag(value));

    public void Set<T>(string key, T? value) where T : INbtWritable => Set(key, value is null ? null : value.Write());

    public void Set<T>(string key, T? value, bool _ = false) where T : struct, Enum =>
        Set(key, value is null ? null : Array.IndexOf(Enum.GetValues<T>(), value.Value));
}
