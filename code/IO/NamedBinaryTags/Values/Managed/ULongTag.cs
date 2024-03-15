using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ULongTag : UnmanagedTag<ulong>
{
    public ULongTag() : this(default)
    {
    }

    public ULongTag(ulong value) : base(BinaryTagType.ULong, 8, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadUInt64();
}
