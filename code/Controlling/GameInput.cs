using Sandbox;

namespace Sandcube.Controlling;

public static class GameInput
{
    public static Vector2 LookDelta => Input.MouseDelta * Preferences.Sensitivity;

    public static bool IsCrouching => Input.Down("Duck");
    public static bool IsRunning => Input.Down("Run");
    public static bool IsJumpPressed => Input.Pressed("Jump");
}
