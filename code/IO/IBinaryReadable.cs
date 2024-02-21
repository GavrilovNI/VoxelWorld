using System.IO;

namespace Sandcube.IO;

public interface IBinaryReadable
{
    void Read(BinaryReader reader);
}
