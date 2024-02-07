using Sandbox;
using Sandcube.Players;

namespace Sandcube.Menus;

public interface IMenu
{
    bool IsStillValid(SandcubePlayer player) => true;
    GameObject CreateScreen(SandcubePlayer player);
}
