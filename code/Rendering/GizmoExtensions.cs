using Sandbox;
using static Sandbox.Gizmo;

namespace VoxelWorld.Rendering;

public static class GizmoExtensions
{
    public static void DrawBboxTraceResult(this GizmoDraw draw, SceneTraceResult traceResult, BBox bounds, Color? endColor = null, float startBackOffset = 0f)
    {
        draw.LineBBox(bounds + traceResult.StartPosition + -traceResult.Direction * startBackOffset);
        draw.Line(traceResult.StartPosition - traceResult.Direction * startBackOffset, traceResult.EndPosition);
        draw.Line(traceResult.StartPosition, traceResult.StartPosition + traceResult.Normal * 100f);
        if(endColor.HasValue)
            draw.Color = endColor.Value;
        draw.LineBBox(bounds + traceResult.EndPosition);
    }

    public static void DrawBboxTraceResult(this GizmoDraw draw, PhysicsTraceResult traceResult, BBox bounds, Color? endColor = null, float startBackOffset = 0f)
    {
        draw.LineBBox(bounds + traceResult.StartPosition + -traceResult.Direction * startBackOffset);
        draw.Line(traceResult.StartPosition - traceResult.Direction * startBackOffset, traceResult.EndPosition);
        draw.Line(traceResult.StartPosition, traceResult.StartPosition + traceResult.Normal * 100f);
        if(endColor.HasValue)
            draw.Color = endColor.Value;
        draw.LineBBox(bounds + traceResult.EndPosition);
    }
}
