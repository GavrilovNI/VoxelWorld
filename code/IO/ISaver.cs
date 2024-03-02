using System.Threading.Tasks;

namespace Sandcube.IO;

public interface ISaver
{
    Task<bool> Save();
}
