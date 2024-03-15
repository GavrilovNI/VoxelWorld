

namespace VoxelWorld.Worlds;

public interface IWorldInitializable
{
    void InitializeWorld(IWorldAccessor world);
}
