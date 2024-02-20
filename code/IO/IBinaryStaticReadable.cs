using System.IO;

namespace Sandcube.IO;

public interface IBinaryStaticReadable<T> where T : IBinaryStaticReadable<T>
{
    static abstract T Read(BinaryReader reader);
}
