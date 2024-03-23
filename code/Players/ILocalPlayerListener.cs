using VoxelWorld.Entities;

namespace VoxelWorld.Players;

public interface ILocalPlayerListener
{
    void OnLocalPlayerCreated(Player player) { }
    void OnLocalPlayerDestroyed(Player player) { }
}
