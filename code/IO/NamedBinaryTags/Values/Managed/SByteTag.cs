using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class SByteTag : UnmanagedTag<sbyte>
{
    public SByteTag(sbyte value = default) : base(BinaryTagType.SByte, 1, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadSByte();
}
