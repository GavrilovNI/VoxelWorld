using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class UIntTag : UnmanagedTag<uint>
{
    public UIntTag() : this(default)
    {
    }

    public UIntTag(uint value) : base(BinaryTagType.UInt, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadUInt32();
}
