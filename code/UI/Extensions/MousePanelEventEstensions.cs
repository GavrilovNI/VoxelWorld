using Sandbox.UI;
using Sandbox;

namespace VoxelWorld.UI.Extensions;

public static class MousePanelEventEstensions
{
    public static MouseButtons GetMouseButton(this MousePanelEvent mousePanelEvent)
    {
        return mousePanelEvent.Button switch
        {
            "mouseleft" => MouseButtons.Left,
            "mousemiddle" => MouseButtons.Middle,
            "mouseright" => MouseButtons.Right,
            _ => MouseButtons.None
        };
    }
}
