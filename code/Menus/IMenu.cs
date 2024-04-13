using Sandbox;

namespace VoxelWorld.Menus;

public interface IMenu
{
    bool IsStillValid() => true;
    GameObject CreateScreen();
}
