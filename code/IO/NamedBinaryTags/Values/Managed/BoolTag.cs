using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class BoolTag : UnmanagedTag<bool>
{
    public BoolTag(bool value = default) : base(BinaryTagType.Bool, 1, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadBoolean();
}
