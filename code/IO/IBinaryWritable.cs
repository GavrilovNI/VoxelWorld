using System.IO;

namespace Sandcube.IO;

public interface IBinaryWritable
{
    void Write(BinaryWriter writer);
}
