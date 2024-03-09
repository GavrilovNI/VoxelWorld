using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class SByteTag : UnmanagedTag<sbyte>
{
    public SByteTag() : this(default)
    {
    }

    public SByteTag(sbyte value) : base(BinaryTagType.SByte, 1, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadSByte();
}
