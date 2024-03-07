using System.IO;

namespace Sandcube.IO.NamedBinaryTags;

public sealed class EmptyTag : BinaryTag
{
    internal EmptyTag() : base(BinaryTagType.Empty)
    {
    }

    public override bool IsDataEmpty => true;

    public override void ReadData(BinaryReader reader) { }
    public override void WriteData(BinaryWriter writer) { }
}
