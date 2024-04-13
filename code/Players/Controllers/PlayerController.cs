using Sandbox;
using System;
using VoxelWorld.Controlling;
using VoxelWorld.Mth;
using VoxelWorld.Physics;

namespace VoxelWorld.Players.Controllers;

public class PlayerController : Component
{
    [Property, RequireComponent] protected CustomCharacterController CharacterController { get; set; } = null!;

    [Property, Group("Eye")] protected GameObject Eye { get; set; } = null!;
    [Property, Group("Eye")] protected float EyeOffsetFromTop { get; set; } = 10f;

    [Property, Group("Collider"), RequireComponent] protected BoxCollider Collider { get; set; } = null!;
    [Property, Group("Collider")] protected float ColliderDefaultHeight { get; set; } = 71f;
    [Property, Group("Collider")] protected float ColliderCrouchingHeight { get; set; } = 56f;
    [Property, Group("Collider")] protected float HeightLerpSpeed { get; set; } = 10f;

    [Property] protected float WalkSpeed { get; set; } = 160f;
    [Property] protected float RunSpeed { get; set; } = 270f;
    [Property] protected float CrouchSpeed { get; set; } = 70f;
    [Property] protected float JumpVelocity { get; set; } = 320f;
    [Property] protected float GroundFriction { get; set; } = 4f;
    [Property] protected float AirFriction { get; set; } = 0.1f;
    [Property] protected float StopSpeed { get; set; } = 140f;
    [Property] protected float AirWishVelocityClamp { get; set; } = 50f;
    [Property] protected float DoublePressTimeThreshold { get; set; } = 0.5f;

    public bool IsCrouchingRequested { get; protected set; } = false;
    public bool IsRunningRequested { get; protected set; } = false;
    public PlayerMoveType MoveType { get; protected set; } = PlayerMoveType.Standing;
    protected TimeSince TimeSinceFirstForwardPressed { get; set; } = float.MaxValue;

    public virtual Vector3 Gravity => CharacterController.Gravity;
    public Vector3 GravityNormal => CharacterController.GravityNormal;

    public Vector3 WishDirection { get; protected set; }
    public Vector3 WishVelocity { get; protected set; }

    protected override void OnAwake()
    {
        ResetEyePositionAndHeight();
    }

    protected override void OnUpdate()
    {
        WishDirection = CalculateWishDirection();
        UpdateMoveType();
        WishVelocity = WishDirection * GetWishSpeed();

        UpdateEyePosition();
    }

    protected virtual void UpdateMoveType()
    {
        IsCrouchingRequested = GameInput.IsCrouching;
        IsRunningRequested = GameInput.IsRunning ||
            (GameInput.IsForwardPressed && TimeSinceFirstForwardPressed <= DoublePressTimeThreshold);

        bool isFullyStanding = CharacterController.Height >= ColliderDefaultHeight || CharacterController.Height.AlmostEqual(ColliderDefaultHeight);

        var upDirection = CharacterController.Transform.Rotation.Up;
        var lookingHorizontalDirection = Eye.Transform.Rotation.Forward.ProjectOnPlane(upDirection);
        var isMoovingForward = lookingHorizontalDirection.AlmostEqual(0f) || WishDirection.ProjectOnPlane(upDirection).Angle(lookingHorizontalDirection) < 90f;

        if(WishDirection.AlmostEqual(0f))
            MoveType = PlayerMoveType.Standing;
        else if(IsCrouchingRequested || !isFullyStanding)
            MoveType = PlayerMoveType.Crouching;
        else if((IsRunningRequested || MoveType == PlayerMoveType.Running) && isMoovingForward)
            MoveType = PlayerMoveType.Running;
        else
            MoveType = PlayerMoveType.Walking;

        if(GameInput.IsForwardPressed)
            TimeSinceFirstForwardPressed = 0f;
    }

    protected override void OnFixedUpdate()
    {
        LerpCollidersHeight();
        Rotate(); // TODO: move to OnUpdate when get not that laggy
        Move();
    }

    protected virtual void ResetEyePositionAndHeight()
    {
        var targetHeight = (IsCrouchingRequested ? ColliderCrouchingHeight : ColliderDefaultHeight);
        CharacterController.Height = targetHeight;
        Collider.Scale = new Vector3(Collider.Scale.x, Collider.Scale.y, targetHeight);
        Collider.Center = new Vector3(0f, 0f, targetHeight / 2f);

        UpdateEyePosition();
    }

    protected virtual void UpdateEyePosition()
    {
        var heightHeight = CharacterController.Height - EyeOffsetFromTop;
        Eye.Transform.Local = Eye.Transform.Local.WithPosition(new Vector3(0f, 0f, heightHeight));
    }

    protected virtual void LerpCollidersHeight()
    {
        var targetHeight = (IsCrouchingRequested ? ColliderCrouchingHeight : ColliderDefaultHeight);
        var currentHeight = CharacterController.Height;

        if(currentHeight == targetHeight)
            return;

        var t = HeightLerpSpeed * Time.Delta / (MathF.Abs(targetHeight - currentHeight) / MathF.Abs(ColliderDefaultHeight - ColliderCrouchingHeight));
        var newHeight = currentHeight.LerpTo(targetHeight, t);

        if(!IsCrouchingRequested)
        {
            var traceResult = CharacterController.TraceDirection(CharacterController.Transform.Rotation.Up * (newHeight - currentHeight)).Run();
            
            if(traceResult.Hit)
                newHeight = currentHeight + traceResult.Distance - 0.01f;
        }

        CharacterController.Height = newHeight;
        Collider.Scale = new Vector3(Collider.Scale.x, Collider.Scale.y, newHeight);
        Collider.Center = new Vector3(0f, 0f, newHeight / 2f);
    }

    protected virtual void Rotate()
    {
        var input = -GameInput.LookDelta.x * Time.Delta;

        Eye.Transform.LocalRotation = Rotation.FromAxis(Transform.Rotation.Up, input) * Eye.Transform.LocalRotation;
    }

    protected virtual void Move()
    {
        CharacterController.CategorizePosition();

        var halfGravityVelocity = Gravity * (Time.Delta * 0.5f);
        var gravityNormal = GravityNormal;

        if(CharacterController.IsOnGround && GameInput.IsJumpPressed)
            Jump();

        if(CharacterController.IsOnGround)
        {
            CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(gravityNormal);
            CharacterController.Accelerate(WishVelocity);
            CharacterController.ApplyFriction(GroundFriction, StopSpeed);

            if(GameInput.IsCrouching)
            {
                var horizontalVelocity = CharacterController.Velocity.ProjectOnPlane(GravityNormal);
                var totalOffset = horizontalVelocity.Normal;

                Vector3[] offsets = new Vector3[] {
                    new Vector3(totalOffset.x, 0f, 0f).Normal,
                    new Vector3(0f, totalOffset.y, 0f).Normal,
                    new Vector3(0f, 0f, totalOffset.z).Normal,
                    totalOffset,
                };

                foreach(var offset in offsets)
                    PreventBlockFalling(offset);

                void PreventBlockFalling(Vector3 offset)
                {
                    if(offset == Vector3.Zero || CharacterController.Velocity == Vector3.Zero)
                        return;

                    CharacterControllerHelper helper = CharacterController.CreateHelper();
                    helper.Velocity = offset;
                    helper.TryMoveWithStep(1f, CharacterController.StepHeight);
                    bool willBeOnGround = CharacterController.IsOnGroundAt(helper.Position, out var groundTraceResult);

                    if(willBeOnGround)
                        return;

                    var startpos = groundTraceResult.EndPosition;
                    helper.Position = startpos;
                    var traceResult = helper.TraceMove(-offset * 1.1f);

                    if(!traceResult.Hit)
                    {
                        CharacterController.Velocity = Vector3.Zero;
                        return;
                    }

                    CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(traceResult.Normal);
                }
            }
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

    protected virtual float GetWishSpeed() => MoveType switch
    {
        PlayerMoveType.Standing => 0f,
        PlayerMoveType.Crouching => CrouchSpeed,
        PlayerMoveType.Walking => WalkSpeed,
        PlayerMoveType.Running => RunSpeed,
        _ => throw new NotSupportedException($"Invalid {nameof(PlayerMoveType)} ({MoveType})")
    };

    protected virtual Vector3 CalculateWishDirection()
    {
        var input = Input.AnalogMove.WithZ(0);
        Vector3 result = input * Eye.Transform.Rotation;
        result = result.ProjectOnPlane(GravityNormal).Normal;
        return result;
    }
}
