

namespace Sandcube.IO.NamedBinaryTags.Values.Unmanaged;

public abstract class UnmanagedTag<T> : ValueTag<T> where T : unmanaged
{
    public byte Size { get; }

    protected internal UnmanagedTag(BinaryTagType type, byte size, T value) : base(type, value)
    {
        Size = size;
    }
}
