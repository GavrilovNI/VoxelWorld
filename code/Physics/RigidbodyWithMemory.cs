using Sandbox;

namespace VoxelWorld.Physics;

public class RigidbodyWithMemory : Rigidbody
{
    private Vector3 _savedVelocity = Vector3.Zero;
    private Vector3 _savedAngularVelocity = Vector3.Zero;

    public new Vector3 Velocity
    {
        get => PhysicsBody.IsValid() ? base.Velocity : _savedVelocity;
        set
        {
            base.Velocity = value;
            _savedVelocity = value;
        }
    }

    public new Vector3 AngularVelocity
    {
        get => PhysicsBody.IsValid() ? base.AngularVelocity : _savedAngularVelocity;
        set
        {
            base.AngularVelocity = value;
            _savedAngularVelocity = value;
        }
    }

    protected override void OnEnabled()
    {
        base.OnEnabled();
        Velocity = _savedVelocity;
        AngularVelocity = _savedAngularVelocity;
    }

    protected override void OnDisabled()
    {
        _savedVelocity = Velocity;
        _savedAngularVelocity = AngularVelocity;
        base.OnDisabled();
    }
}
