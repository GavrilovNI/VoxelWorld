using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class DoubleTag : UnmanagedTag<double>
{
    public DoubleTag() : this(default)
    {
    }

    public DoubleTag(double value) : base(BinaryTagType.Double, 8, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadDouble();
}
