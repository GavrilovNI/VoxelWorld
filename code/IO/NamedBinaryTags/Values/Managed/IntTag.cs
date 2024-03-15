using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class IntTag : UnmanagedTag<int>
{
    public IntTag() : this(default)
    {
    }

    public IntTag(int value) : base(BinaryTagType.Int, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadInt32();
}
