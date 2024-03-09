using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class BoolTag : UnmanagedTag<bool>
{
    public BoolTag() : this(default)
    {
    }

    public BoolTag(bool value) : base(BinaryTagType.Bool, 1, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadBoolean();
}
