using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class UShortTag : UnmanagedTag<ushort>
{
    public UShortTag() : this(default)
    {
    }

    public UShortTag(ushort value) : base(BinaryTagType.UShort, 2, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadUInt16();
}
