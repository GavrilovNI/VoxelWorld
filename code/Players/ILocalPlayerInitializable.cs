using VoxelWorld.Entities;

namespace VoxelWorld.Players;

public interface ILocalPlayerInitializable
{
    void InitializeLocalPlayer(Player player);
}
