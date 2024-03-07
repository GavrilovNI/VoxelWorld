using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ULongTag : UnmanagedTag<ulong>
{
    public ULongTag() : this(default)
    {
    }

    public ULongTag(ulong value) : base(BinaryTagType.ULong, 8, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadUInt64();
}
