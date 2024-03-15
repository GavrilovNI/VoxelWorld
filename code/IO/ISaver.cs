using System.Threading.Tasks;

namespace VoxelWorld.IO;

public interface ISaver
{
    Task<bool> Save();
}
