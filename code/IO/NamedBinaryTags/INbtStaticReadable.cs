

namespace Sandcube.IO.NamedBinaryTags;

public interface INbtStaticReadable<T> where T : INbtStaticReadable<T>
{
    static abstract T Read(BinaryTag tag);
}
