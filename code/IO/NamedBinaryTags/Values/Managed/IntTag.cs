﻿using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class IntTag : UnmanagedTag<int>
{
    public IntTag() : this(default)
    {
    }

    public IntTag(int value) : base(BinaryTagType.Int, 4, value)
    {
    }

    public override void WriteData(BinaryWriter writer) => writer.Write(Value);
    public override void ReadData(BinaryReader reader) => Value = reader.ReadInt32();
}
