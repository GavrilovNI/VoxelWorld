using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class UIntTag : UnmanagedTag<uint>
{
    public UIntTag() : this(default)
    {
    }

    public UIntTag(uint value) : base(BinaryTagType.UInt, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadUInt32();
}
