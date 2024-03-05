﻿using System.IO;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public sealed class DecimalTag : UnmanagedTag<decimal>
{
    public DecimalTag(decimal value = default) : base(BinaryTagType.Decimal, 16, value)
    {
    }

    public override void Write(BinaryWriter writer) => writer.Write(Value);
    public override void Read(BinaryReader reader) => Value = reader.ReadDecimal();
}
