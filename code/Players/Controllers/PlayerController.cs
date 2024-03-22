using Sandbox;
using VoxelWorld.Controlling;
using VoxelWorld.Mth;

namespace VoxelWorld.Players.Controllers;

public class PlayerController : Component
{
    [Property] protected CharacterController CharacterController { get; set; } = null!;
    [Property] protected GameObject Eye { get; set; } = null!;
    [Property] protected float WalkSpeed { get; set; } = 160f;
    [Property] protected float RunSpeed { get; set; } = 270f;
    [Property] protected float CrouchSpeed { get; set; } = 70f;
    [Property] protected float JumpVelocity { get; set; } = 320f;
    [Property] protected float GroundFriction { get; set; } = 4f;
    [Property] protected float AirFriction { get; set; } = 0.1f;
    [Property] protected float StopSpeed { get; set; } = 140f;
    [Property] protected float AirWishVelocityClamp { get; set; } = 50f;

    public bool IsCrouching { get; protected set; } = false;

    public virtual Vector3 Gravity => Scene.PhysicsWorld.Gravity;
    public Vector3 GravityNormal
    {
        get
        {
            var gravity = Gravity;
            var result = gravity.Normal;
            if(result.AlmostEqual(0))
                return Vector3.Down;
            return result;
        }
    }

    public Vector3 WishVelocity { get; protected set; }

    protected override void OnAwake()
    {
        CharacterController ??= Components.Get<CharacterController>();
    }

    protected override void OnUpdate()
    {
        IsCrouching = GameInput.IsCrouching;
        WishVelocity = CalculateWishVelocity();
    }

    protected override void OnFixedUpdate()
    {
        Rotate(); // TODO: move to OnUpdate when get not that laggy
        Move();
    }

    protected virtual void Rotate()
    {
        var input = -GameInput.LookDelta.x * Time.Delta;

        Eye.Transform.LocalRotation = Rotation.FromAxis(Transform.Rotation.Up, input) * Eye.Transform.LocalRotation;
    }

    protected virtual void Move()
    {
        var halfGravityVelocity = Gravity * (Time.Delta * 0.5f);
        var gravityNormal = GravityNormal;

        if(CharacterController.IsOnGround && GameInput.IsJumpPressed)
            Jump();

        if(CharacterController.IsOnGround)
        {
            CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(gravityNormal);
            CharacterController.Accelerate(WishVelocity);
            CharacterController.ApplyFriction(GroundFriction, StopSpeed);
        }
        else
        {
            CharacterController.Velocity += halfGravityVelocity;
            CharacterController.Accelerate(WishVelocity.ClampLength(AirWishVelocityClamp));
            CharacterController.ApplyFriction(AirFriction, StopSpeed);
        }

        CharacterController.Move();

        if(CharacterController.IsOnGround)
            CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(gravityNormal);
        else
            CharacterController.Velocity += halfGravityVelocity;
    }

    protected virtual void Jump()
    {
        CharacterController.Punch(-GravityNormal * JumpVelocity);
    }

    protected virtual float GetWishSpeed(Vector3 input)
    {
        if(IsCrouching)
            return CrouchSpeed;

        bool goingForward = input.x > 0;
        if(goingForward && GameInput.IsRunning)
            return RunSpeed;

        return WalkSpeed;
    }

    protected virtual Vector3 CalculateWishVelocity()
    {
        var input = Input.AnalogMove.WithZ(0);
        Vector3 result = input * Eye.Transform.Rotation;
        result = result.ProjectOnPlane(GravityNormal).Normal;
        result *= GetWishSpeed(input);
        return result;
    }
}
