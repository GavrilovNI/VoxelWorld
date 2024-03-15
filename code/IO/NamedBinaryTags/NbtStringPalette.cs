using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags;

public class NbtStringPalette : INbtWritable, INbtStaticReadable<NbtStringPalette>
{
    private int _nextValue = 0;
    private readonly Dictionary<string, int> _palette = new();
    private readonly Dictionary<int, string> _reversedPalette = new();


    public bool HasValue(int id) => _reversedPalette.ContainsKey(id);
    public bool HasId(string value) => _palette.ContainsKey(value);

    public int GetOrAddId(string value)
    {
        ArgumentNullException.ThrowIfNull(nameof(value));

        if(value.Length == 0)
            return -1;

        if(_palette.TryGetValue(value, out var result))
            return result;

        result = _nextValue++;
        _palette[value] = result;
        _reversedPalette[result] = value;
        return result;
    }

    public string GetValue(int id, string defaultValue = "")
    {
        if(_reversedPalette.TryGetValue(id, out var result))
            return result;
        return defaultValue;
    }

    public BinaryTag Write()
    {
        ListTag tag = new();
        for(int i = 0; i < _nextValue; ++i)
        {
            var value = _reversedPalette.GetValueOrDefault(i, string.Empty);
            tag.Add(value);
        }
        return tag;
    }

    public static NbtStringPalette Read(BinaryTag tag)
    {
        ListTag listTag = tag.To<ListTag>();

        if(listTag.TagsType != BinaryTagType.String)
            return new();

        NbtStringPalette result = new()
        {
            _nextValue = listTag.Count
        };

        for(int i = 0; i < listTag.Count; ++i)
        {
            var value = listTag.Get<string>(i);
            if(value.Length != 0)
            {
                result._palette[value] = i;
                result._reversedPalette[i] = value;
            }
        }
        return result;
    }
}

public static class NbtStringPaletteExtensions
{
    public static void WriteId(this NbtStringPalette? palette, BinaryWriter writer, string value)
    {
        if(palette is null)
            writer.Write(value);
        else
            writer.Write(palette.GetOrAddId(value));
    }

    public static string ReadValue(this NbtStringPalette? palette, BinaryReader reader, string defaultValue = "")
    {
        if(palette is null)
            return reader.ReadString();
        else
            return palette.GetValue(reader.ReadInt32(), defaultValue);
    }
}
