using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class UShortTag : UnmanagedTag<ushort>
{
    public UShortTag(ushort value = default) : base(BinaryTagType.UShort, 2, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadUInt16();
}
