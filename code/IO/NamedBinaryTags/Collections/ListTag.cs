using VoxelWorld.IO.NamedBinaryTags.Values;
using VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VoxelWorld.IO.NamedBinaryTags.Collections;

public sealed class ListTag : NbtReadCollection<int>, IEnumerable<BinaryTag>
{
    public BinaryTagType? TagsType { get; private set; }

    public int Count { get; private set; } = 0;
    public override bool IsDataEmpty => Count == 0 || _tags.Values.All(t => t.IsDataEmpty);

    private readonly Dictionary<int, BinaryTag> _tags = new();

    public ListTag() : this(null)
    {

    }

    public ListTag(BinaryTagType? valueType) : base(BinaryTagType.List)
    {
        TagsType = valueType;
    }

    protected override bool TryGetTag(int index, out BinaryTag tag) => _tags.TryGetValue(index, out tag!);

    private BinaryTag GetTagOrCreate(int index)
    {
        if(!TagsType.HasValue)
            return BinaryTag.Empty;

        if(_tags.TryGetValue(index, out var tag))
            return tag;

        tag = BinaryTag.CreateTag(TagsType!.Value);
        this[index] = tag;
        return tag;
    }

    public void RemoveAt(int index)
    {
        if(index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "out of range");
        _tags.Remove(index);

        if(_tags.Count == 0)
        {
            TagsType = null;
            Count = 0;
        }
        else
        {
            Count = _tags.Max(t => t.Key) + 1;
        }
    }

    public void Clear()
    {
        _tags.Clear();
        TagsType = null;
        Count = 0;
    }

    public BinaryTag this[int index]
    {
        get => GetTagOrCreate(index);
        set
        {
            if(index >= Count)
            {
                Insert(index, value);
            }
            else
            {
                if(value is EmptyTag)
                    return;

                AssertType(value);

                _tags[index] = value;
                TagsType = value.Type;
            }
        }
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette)
    {
        writer.Write(Count);
        if(Count > 0)
        {
            BinaryTag.WriteType(writer, TagsType!.Value);
            for(int i = 0; i < Count; ++i)
                GetTagOrCreate(i).WriteData(writer, palette);
        }
    }

    public override void ReadData(BinaryReader reader, NbtStringPalette? palette)
    {
        Clear();

        Count = reader.ReadInt32();
        if(Count == 0)
            return;

        TagsType = BinaryTag.ReadType(reader);
        for(int i = 0; i < Count; ++i)
        {
            var tag = BinaryTag.ReadTagData(reader, TagsType.Value, palette);
            if(!tag.IsDataEmpty)
                this[i] = tag;
        }

        if(TagsType == BinaryTagType.Empty)
            Clear();
    }

    public IEnumerator<BinaryTag> GetEnumerator()
    {
        for(int i = 0; i < Count; ++i)
            yield return GetTagOrCreate(i);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



    public void Add(BinaryTag tag) => Insert(Count, tag);

    public void Add(byte value) => Add(new ByteTag(value));

    public void Add(sbyte value) => Add(new SByteTag(value));

    public void Add(short value) => Add(new ShortTag(value));

    public void Add(ushort value) => Add(new UShortTag(value));

    public void Add(int value) => Add(new IntTag(value));

    public void Add(uint value) => Add(new UIntTag(value));

    public void Add(long value) => Add(new LongTag(value));

    public void Add(ulong value) => Add(new ULongTag(value));

    public void Add(float value) => Add(new FloatTag(value));

    public void Add(double value) => Add(new DoubleTag(value));

    public void Add(decimal value) => Add(new DecimalTag(value));

    public void Add(bool value) => Add(new BoolTag(value));

    public void Add(char value) => Add(new CharTag(value));

    public void Add(string value) => Add(new StringTag(value));

    public void Add<T>(T value) where T : INbtWritable => Add(value.Write());

    public void Add<T>(T value, bool _ = false) where T : struct, Enum =>
        Add(Array.IndexOf(Enum.GetValues<T>(), value));


    public void Set(int index, BinaryTag? tag)
    {
        if(tag is null)
            RemoveAt(index);
        else
            this[index] = tag;
    }

    public void Set(int index, byte? value) => Set(index, value is null ? null : new ByteTag(value.Value));

    public void Set(int index, sbyte? value) => Set(index, value is null ? null : new SByteTag(value.Value));

    public void Set(int index, short? value) => Set(index, value is null ? null : new ShortTag(value.Value));

    public void Set(int index, ushort? value) => Set(index, value is null ? null : new UShortTag(value.Value));

    public void Set(int index, int? value) => Set(index, value is null ? null : new IntTag(value.Value));

    public void Set(int index, uint? value) => Set(index, value is null ? null : new UIntTag(value.Value));

    public void Set(int index, long? value) => Set(index, value is null ? null : new LongTag(value.Value));

    public void Set(int index, ulong? value) => Set(index, value is null ? null : new ULongTag(value.Value));

    public void Set(int index, float? value) => Set(index, value is null ? null : new FloatTag(value.Value));

    public void Set(int index, double? value) => Set(index, value is null ? null : new DoubleTag(value.Value));

    public void Set(int index, decimal? value) => Set(index, value is null ? null : new DecimalTag(value.Value));

    public void Set(int index, bool? value) => Set(index, value is null ? null : new BoolTag(value.Value));

    public void Set(int index, char? value) => Set(index, value is null ? null : new CharTag(value.Value));

    public void Set(int index, string? value) => Set(index, value is null ? null : new StringTag(value));

    public void Set<T>(int index, T? value) where T : INbtWritable => Set(index, value is null ? null : value.Write());

    public void Set<T>(int index, T? value, bool _ = false) where T : struct, Enum =>
        Set(index, value is null ? null : Array.IndexOf(Enum.GetValues<T>(), value.Value));


    public void Insert(int index, BinaryTag tag)
    {
        if(index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"index can't be less than 0");

        if(tag is EmptyTag)
            return;

        AssertType(tag);

        for(int i = Count; i > index; i--)
        {
            if(_tags.TryGetValue(i - 1, out var prevValue))
                _tags[i] = prevValue;
            else
                _tags.Remove(i);
        }

        _tags[index] = tag;

        if(Count == 0)
            TagsType = tag.Type;

        Count = Math.Max(Count, index) + 1;
    }

    public void Insert(int index, byte value) => Insert(index, new ByteTag(value));

    public void Insert(int index, sbyte value) => Insert(index, new SByteTag(value));

    public void Insert(int index, short value) => Insert(index, new ShortTag(value));

    public void Insert(int index, ushort value) => Insert(index, new UShortTag(value));

    public void Insert(int index, int value) => Insert(index, new IntTag(value));

    public void Insert(int index, uint value) => Insert(index, new UIntTag(value));

    public void Insert(int index, long value) => Insert(index, new LongTag(value));

    public void Insert(int index, ulong value) => Insert(index, new ULongTag(value));

    public void Insert(int index, float value) => Insert(index, new FloatTag(value));

    public void Insert(int index, double value) => Insert(index, new DoubleTag(value));

    public void Insert(int index, decimal value) => Insert(index, new DecimalTag(value));

    public void Insert(int index, bool value) => Insert(index, new BoolTag(value));

    public void Insert(int index, char value) => Insert(index, new CharTag(value));

    public void Insert(int index, string value) => Insert(index, new StringTag(value));

    public void Insert<T>(int index, T value) where T : INbtWritable => Insert(index, value.Write());

    public void Insert<T>(int index, T value, bool _ = false) where T : struct, Enum =>
        Insert(index, Array.IndexOf(Enum.GetValues<T>(), value));


    private void AssertType(BinaryTag tag, [CallerArgumentExpression(nameof(tag))] string? paramName = null)
    {
        if(tag.Type == BinaryTagType.Empty)
            throw new ArgumentException($"can't add {nameof(EmptyTag)} to {nameof(ListTag)}", paramName);

        if(!TagsType.HasValue)
            return;

        if(tag.Type != TagsType)
            throw new ArgumentException($"{paramName}'s type ({tag.Type}) is not {nameof(ListTag)}'s type ({TagsType})", paramName);
    }
}
