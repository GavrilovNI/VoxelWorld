using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class LongTag : UnmanagedTag<long>
{
    public LongTag() : this(default)
    {
    }

    public LongTag(long value) : base(BinaryTagType.Long, 8, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadInt64();
}
