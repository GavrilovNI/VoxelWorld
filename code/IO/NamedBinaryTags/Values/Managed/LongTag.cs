using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class LongTag : UnmanagedTag<long>
{
    public LongTag(long value = default) : base(BinaryTagType.Long, 8, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadInt64();
}
