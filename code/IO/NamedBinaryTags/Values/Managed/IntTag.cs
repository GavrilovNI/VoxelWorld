using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class IntTag : UnmanagedTag<int>
{
    public IntTag(int value = default) : base(BinaryTagType.Int, 4, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadInt32();
}
