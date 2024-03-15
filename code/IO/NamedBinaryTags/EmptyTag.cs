using System.IO;

namespace VoxelWorld.IO.NamedBinaryTags;

public sealed class EmptyTag : BinaryTag
{
    internal EmptyTag() : base(BinaryTagType.Empty)
    {
    }

    public override bool IsDataEmpty => true;

    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) { }
    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) { }
}
