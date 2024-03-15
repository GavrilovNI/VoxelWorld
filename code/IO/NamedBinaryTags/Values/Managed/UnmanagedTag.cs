using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Unmanaged;

public abstract class UnmanagedTag<T> : ValueTag<T> where T : unmanaged, IEquatable<T>
{
    public override bool IsDataEmpty => Value.Equals(default);

    public byte Size { get; }

    protected internal UnmanagedTag(BinaryTagType type, byte size, T value) : base(type, value)
    {
        Size = size;
    }

    public override bool Equals(T other) => Value.Equals(other);
}
