using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ShortTag : UnmanagedTag<short>
{
    public ShortTag() : this(default)
    {
    }

    public ShortTag(short value) : base(BinaryTagType.Short, 2, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadInt16();
}
