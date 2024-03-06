using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class CharTag : UnmanagedTag<char>
{
    public CharTag(char value = default) : base(BinaryTagType.Char, 2, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadChar();
}
