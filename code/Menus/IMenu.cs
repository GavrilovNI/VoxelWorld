using Sandbox;
using VoxelWorld.Entities;

namespace VoxelWorld.Menus;

public interface IMenu
{
    bool IsStillValid() => true;
    GameObject CreateScreen();
}
