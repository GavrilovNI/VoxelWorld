using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class LongTag : UnmanagedTag<long>
{
    public LongTag() : this(default)
    {
    }

    public LongTag(long value) : base(BinaryTagType.Long, 8, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadInt64();
}
