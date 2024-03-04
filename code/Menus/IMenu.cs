using Sandbox;
using Sandcube.Entities;

namespace Sandcube.Menus;

public interface IMenu
{
    bool IsStillValid() => true;
    GameObject CreateScreen();
}
