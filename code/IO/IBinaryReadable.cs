using System.IO;

namespace Sandcube.IO;

public interface IBinaryReadable<T> where T : IBinaryReadable<T>
{
    void Read(BinaryReader reader);
}
