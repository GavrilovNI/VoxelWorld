using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class DecimalTag : UnmanagedTag<decimal>
{
    public DecimalTag() : this(default)
    {
    }

    public DecimalTag(decimal value) : base(BinaryTagType.Decimal, 16, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadDecimal();
}
