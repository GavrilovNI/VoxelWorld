using Sandbox;
using Sandcube.Controlling;
using Sandcube.Mth;
using System;

namespace Sandcube.Players.Controllers;

public class VerticalCameraController : Component
{
    [Property] protected CameraComponent Camera { get; set; } = null!;
    [Property] protected float UpClamp { get; set; } = 90;
    [Property] protected float DownClamp { get; set; } = -90;


    protected override void OnFixedUpdate()
    {
        Rotate(); // TODO: move to OnUpdate when get not that laggy
    }

    protected virtual void Rotate()
    {
        var input = -GameInput.LookDelta.y * Time.Delta;

        var currentAngle = Transform.Rotation.Forward.SignedAngle(Camera.Transform.Rotation.Forward, Transform.Rotation.Right);
        var newAngle = Math.Clamp(input + currentAngle, DownClamp, UpClamp);

        var newRotation = Rotation.FromAxis(Transform.Rotation.Right, newAngle) * Transform.Rotation;
        Camera.Transform.Rotation = Rotation.LookAt(newRotation.Forward, newRotation.Up);
    }   
}
