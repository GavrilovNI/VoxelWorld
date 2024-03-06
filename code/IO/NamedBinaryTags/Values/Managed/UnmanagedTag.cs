using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public abstract class UnmanagedTag<T> : ValueTag<T> where T : unmanaged, IEquatable<T>
{
    public override bool IsEmpty => Value.Equals(default);

    public byte Size { get; }

    protected internal UnmanagedTag(BinaryTagType type, byte size, T value) : base(type, value)
    {
        Size = size;
    }
}
