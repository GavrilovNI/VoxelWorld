using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class ShortTag : UnmanagedTag<short>
{
    public ShortTag(short value = default) : base(BinaryTagType.Short, 2, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadInt16();
}
