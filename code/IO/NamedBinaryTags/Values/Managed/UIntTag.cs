using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class UIntTag : UnmanagedTag<uint>
{
    public UIntTag(uint value = default) : base(BinaryTagType.UInt, 4, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadUInt32();
}
