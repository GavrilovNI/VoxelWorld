using Sandcube.Data;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Values;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Sandcube;

public readonly record struct Id : INbtWritable, INbtStaticReadable<Id>
{
    private const char Underscore = '_';
    public static readonly Regex Regex = new("^[a-z_]+$");

    public readonly string Name { get; init; }

    public Id(string name)
    {
        if(!IsValidString(name))
            throw new ArgumentException($"{nameof(name)} {name} should match {nameof(Regex)}");
        Name = name;
    }

    public static bool TryFromString(string name, out Id id)
    {
        bool canConvert = IsValidString(name);
        id = canConvert ? new(name) : default;
        return canConvert;
    }

    public static bool TryFromCamelCase(string value, out Id id)
    {
        StringBuilder builder = new();

        for(int i = 0; i < value.Length; ++i)
        {
            char c = value[i];

            if(Char.IsUpper(c))
            {
                c = Char.ToLower(c);
                if(i != 0 && value[i - 1] != '_')
                    builder.Append(Underscore);
            }
            builder.Append(c);
        }

        return TryFromString(builder.ToString(), out id);
    }

    public static bool IsValidString(string name) => !string.IsNullOrEmpty(name) && Regex.IsMatch(name);

    public static implicit operator string(Id id) => id.ToString();
    public static explicit operator Id(string id) => new(id);

    public override int GetHashCode() => Name.GetConsistentHashCode();

    public readonly override string ToString()
    {
        return Name;
    }

    public BinaryTag Write() => new StringTag(Name);
    public static Id Read(BinaryTag tag) => new(tag.To<StringTag>().Value);
}
