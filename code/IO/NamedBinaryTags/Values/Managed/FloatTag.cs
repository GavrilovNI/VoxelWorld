using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class FloatTag : UnmanagedTag<float>
{
    public FloatTag(float value = default) : base(BinaryTagType.Float, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadSingle();
}
