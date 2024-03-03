using Sandbox;
using Sandcube.Entities;

namespace Sandcube.Menus;

public interface IMenu
{
    bool IsStillValid(Player player) => true;
    GameObject CreateScreen(Player player);
}
