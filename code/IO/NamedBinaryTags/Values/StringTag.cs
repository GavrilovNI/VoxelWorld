using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values;

public sealed class StringTag : ValueTag<string>
{
    public override string Value { get => base.Value; set => base.Value = value ?? string.Empty; }

    public override bool IsDataEmpty => Value.Length == 0;

    public StringTag() : this(string.Empty)
    {
    }

    public StringTag(string value) : base(BinaryTagType.String, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) =>
        palette.WriteId(writer, Value);

    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) =>
        Value = palette.ReadValue(reader);

    public override bool Equals(string? other) => other is null ? Value.Length == 0 : Value.Equals(other);
}
