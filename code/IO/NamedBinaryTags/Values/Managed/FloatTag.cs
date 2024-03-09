using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class FloatTag : UnmanagedTag<float>
{
    public FloatTag() : this(default)
    {
    }

    public FloatTag(float value) : base(BinaryTagType.Float, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer, NbtStringPalette? palette) => writer.Write(Value);
    public override void ReadData(BinaryReader reader, NbtStringPalette? palette) => Value = reader.ReadSingle();
}
