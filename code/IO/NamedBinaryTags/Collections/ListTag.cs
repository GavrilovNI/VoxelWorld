﻿using Sandcube.IO.NamedBinaryTags.Values;
using Sandcube.IO.NamedBinaryTags.Values.Unmanaged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Sandcube.IO.NamedBinaryTags.Collections;

public sealed class ListTag : NbtReadCollection<int>, IEnumerable<BinaryTag>
{
    public BinaryTagType? TagsType { get; private set; }

    public int Count => _tags.Count;
    public override bool IsEmpty => Count == 0;

    private readonly List<BinaryTag> _tags = new();

    public ListTag(BinaryTagType? valueType = null) : base(BinaryTagType.List)
    {
        TagsType = valueType;
    }

    public override bool HasTag(int key) => key >= 0 && key < _tags.Count;
    public override BinaryTag GetTag(int key) => _tags[key];

    public void RemoveAt(int index)
    {
        AssertIndex(index, false);
        _tags.RemoveAt(index);
        if(_tags.Count == 0)
            TagsType = null;
    }

    public void Clear()
    {
        _tags.Clear();
        TagsType = null;
    }

    public BinaryTag this[int index]
    {
        get => _tags[index];
        set
        {
            AssertIndex(index, false);
            AssertType(value);
            _tags[index] = value;
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        long startPosition = writer.BaseStream.Position;
        writer.Write(0L); // writing size

        writer.Write(_tags.Count);
        if(_tags.Count > 0)
        {
            BinaryTag.WriteType(writer, TagsType!.Value);
            foreach(var tag in _tags)
                tag.WriteData(writer);
        }

        long size = writer.BaseStream.Position - startPosition - 8;
        using(StreamPositionRememberer rememberer = writer)
        {
            writer.BaseStream.Position = startPosition;
            writer.Write(size);
        }
    }

    public override void ReadData(BinaryReader reader)
    {
        Clear();

        long _ = reader.ReadInt64(); // reading size

        int tagsCount = reader.ReadInt32();
        if(tagsCount == 0)
            return;

        TagsType = BinaryTag.ReadType(reader);
        for(int i = 0; i < tagsCount; ++i)
        {
            var tag = BinaryTag.Read(reader, TagsType.Value);
            Add(tag);
        }
    }

    public static void Skip(BinaryReader reader)
    {
        long size = reader.ReadInt64();
        reader.BaseStream.Position += size;
    }

    public IEnumerator<BinaryTag> GetEnumerator() => _tags.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



    public void Add(BinaryTag tag)
    {
        if(!TagsType.HasValue)
            TagsType = tag.Type;

        AssertType(tag);
        _tags.Add(tag);
    }

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



    public void Insert(int index, BinaryTag tag)
    {
        AssertIndex(index, true);
        AssertType(tag);

        if(!TagsType.HasValue)
            TagsType = tag.Type;

        _tags.Insert(index, tag);
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


    private void AssertIndex(int index, bool inserting, [CallerArgumentExpression(nameof(index))] string? paramName = null)
    {
        if(index < 0 || (inserting ? index > Count : index >= Count))
            throw new ArgumentOutOfRangeException(paramName);
    }

    private void AssertType(BinaryTag tag, [CallerArgumentExpression(nameof(tag))] string? paramName = null)
    {
        if(!TagsType.HasValue)
            return;

        if(tag.Type != TagsType)
            throw new ArgumentException($"{paramName}'s type ({tag.Type}) is not {TagsType}", paramName);
    }
}
