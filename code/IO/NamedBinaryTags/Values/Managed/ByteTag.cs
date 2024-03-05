using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ByteTag : UnmanagedTag<byte>
{
    public ByteTag(byte value = default) : base(BinaryTagType.Byte, 1, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadByte();
}
