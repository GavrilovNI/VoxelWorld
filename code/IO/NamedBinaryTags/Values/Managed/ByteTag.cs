using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ByteTag : UnmanagedTag<byte>
{
    public ByteTag() : this(default)
    {
    }

    public ByteTag(byte value) : base(BinaryTagType.Byte, 1, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadByte();
}
