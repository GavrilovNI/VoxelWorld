using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class DoubleTag : UnmanagedTag<double>
{
    public DoubleTag(double value = default) : base(BinaryTagType.Double, 8, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadDouble();
}
