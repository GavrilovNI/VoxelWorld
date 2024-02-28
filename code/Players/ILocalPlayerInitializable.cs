using Sandcube.Entities;

namespace Sandcube.Players;

public interface ILocalPlayerInitializable
{
    void InitializeLocalPlayer(Entity player);
}
