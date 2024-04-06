using Sandbox;

namespace VoxelWorld.SandboxExtensions;

public static class GameObjectExtensions
{
    public static void SetParentCalmly(this GameObject gameObject, GameObject parent, bool keepWorldPosition = true)
    {
        var wasEnabled = gameObject.Enabled;
        gameObject.Enabled = false;
        gameObject.SetParent(parent, keepWorldPosition);
        gameObject.Enabled = wasEnabled;
    }
}
