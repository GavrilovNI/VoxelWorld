using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values;

public sealed class StringTag : ValueTag<string>
{
    public override bool IsEmpty => Value.Length == 0;

    public StringTag(string value = "") : base(BinaryTagType.String, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadString();
}
