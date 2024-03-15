using Sandbox;

namespace VoxelWorld.Controlling;

public static class GameInput
{
    public static Vector2 LookDelta => Input.MouseDelta * Preferences.Sensitivity;

    public static bool IsCrouching => Input.Down("Duck");
    public static bool IsRunning => Input.Down("Run");
    public static bool IsJumpPressed => Input.Pressed("Jump");
    public static bool IsHandSwapPressed => Input.Pressed("HandSwap");
    public static bool IsDropPressed => Input.Pressed("Drop");

    public static bool IsSlotPressed(int slotIndex) => Input.Pressed($"Slot{slotIndex + 1}");
    public static bool IsSlotPrevPressed => Input.Pressed("SlotPrev");
    public static bool IsSlotNextPressed => Input.Pressed("SlotNext");
}
