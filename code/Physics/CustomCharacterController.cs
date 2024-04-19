using Sandbox;
using VoxelWorld.Mth;

namespace VoxelWorld.Physics;

public class CustomCharacterController : Component
{
    [Range(0f, 200f, 0.01f, true, true)]
    [Property]
    public float Radius { get; set; } = 16f;


    [Range(0f, 200f, 0.01f, true, true)]
    [Property]
    public float Height { get; set; } = 64f;


    [Range(0f, 50f, 0.01f, true, true)]
    [Property]
    public float StepHeight { get; set; } = 18f;


    [Range(0f, 90f, 0.01f, true, true)]
    [Property]
    public float GroundAngle { get; set; } = 45f;


    [Range(0f, 64f, 0.01f, true, true)]
    [Property]
    public float Acceleration { get; set; } = 10f;


    [Range(0f, 1f, 0.01f, true, true)]
    [Property]
    [Description("When jumping into walls, should we bounce off or just stop dead?")]
    public float Bounciness { get; set; } = 0.3f;

    [Property]
    public TagSet IgnoreLayers { get; set; } = new();

    [Sync]
    [Property]
    public Vector3 Velocity { get; set; }

    [Sync]
    public bool IsOnGround { get; protected set; }

    [Property]
    public bool UseSceneGravity { get; set; } = true;

    private Vector3 _gravity = Vector3.Down * 850;
    [Property]
    public Vector3 Gravity
    {
        get => UseSceneGravity ? Scene.PhysicsWorld.Gravity : _gravity;
        set => _gravity = value;
    }

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

    [Property]
    public bool TryUnstuckOnMove { get; protected set; } = true;


    public GameObject? GroundObject { get; protected set; }
    public Collider? GroundCollider { get; protected set; }


    public BBox BoundingBox => new(new Vector3(0f - Radius, 0f - Radius, 0f), new Vector3(Radius, Radius, Height));


    private int _unstuckTries = 0;


    public virtual void Accelerate(in Vector3 vector)
    {
        Velocity = Velocity.WithAcceleration(vector, Acceleration * Time.Delta);
    }

    public virtual void ApplyFriction(float frictionAmount, float stopSpeed = 140f)
    {
        float velocityLength = Velocity.Length;
        if(velocityLength < 0.01f)
            return;

        float stopVelocityLength = ((velocityLength < stopSpeed) ? stopSpeed : velocityLength);
        float stopDelta = stopVelocityLength * Time.Delta * frictionAmount;
        float newVelocityLength = velocityLength - stopDelta;
        if(newVelocityLength < 0f)
            newVelocityLength = 0f;

        if(newVelocityLength != velocityLength)
            Velocity = Velocity.Normal * newVelocityLength;
    }

    protected virtual SceneTrace BuildTrace(in SceneTrace source) => source.Size(BoundingBox).WithoutTags(IgnoreLayers).IgnoreGameObjectHierarchy(GameObject);
    public SceneTrace BuildTrace(in Vector3 from, in Vector3 to) => BuildTrace(Scene.Trace.Ray(in from, in to));
    public SceneTrace TraceDirection(in Vector3 direction) => BuildTrace(GameObject.Transform.Position, GameObject.Transform.Position + direction);

    public virtual bool IsOnGroundAt(in Vector3 position, out SceneTraceResult traceResult)
    {
        Vector3 to = position + Vector3.Down * 2f;
        traceResult = BuildTrace(in position, to).Run();
        return traceResult.Hit && Vector3.GetAngle(-GravityNormal, in traceResult.Normal) <= GroundAngle;
    }

    public virtual void CategorizePosition()
    {
        bool wasOnGround = IsOnGround;
        if(!wasOnGround && Velocity.z > 40f)
        {
            ClearGround();
            return;
        }

        if(!IsOnGroundAt(Transform.Position, out var traceResult))
        {
            ClearGround();
            return;
        }

        IsOnGround = true;
        GroundObject = traceResult.GameObject;
        GroundCollider = traceResult.Shape?.Collider as Collider;

        if(wasOnGround && !traceResult.StartedSolid && traceResult.Fraction > 0f && traceResult.Fraction < 1f)
            Transform.Position = traceResult.EndPosition + traceResult.Normal * 0.01f;
    }

    public virtual void Punch(in Vector3 amount)
    {
        ClearGround();
        Velocity += amount;
    }

    protected virtual void ClearGround()
    {
        IsOnGround = false;
        GroundObject = null;
        GroundCollider = null;
    }

    public virtual void Move()
    {
        if(TryUnstuckOnMove && !TryUnstuck())
            return;

        Move(IsOnGround);
        CategorizePosition();
    }

    protected virtual void Move(bool useStep)
    {
        useStep &= IsOnGround;

        if(useStep)
            Velocity = Velocity.ProjectOnPlane(GravityNormal);

        if(!TryMove(Velocity, Time.Delta, useStep, out var helper))
        {
            Velocity = Vector3.Zero;
            return;
        }

        Velocity = helper.Velocity;
    }

    public virtual void MoveTo(in Vector3 targetPosition, bool useStep)
    {
        if(TryUnstuckOnMove && !TryUnstuck())
            return;

        Vector3 positionOffset = targetPosition - Transform.Position;
        TryMove(positionOffset, 1f, useStep, out _, false);
    }

    protected virtual bool TryMove(in Vector3 velocity, float timeDelta, bool useStep, out CharacterControllerHelper helper, bool bounce = true)
    {
        helper = CreateHelper(Transform.Position, velocity, bounce);

        if(velocity.Length.AlmostEqual(0f))
            return false;

        if(TryUnstuckOnMove && !TryUnstuck())
            return false;

        if(useStep)
            helper.TryMoveWithStep(timeDelta, StepHeight);
        else
            helper.TryMove(timeDelta);

        Transform.Position = helper.Position;
        CategorizePosition();
        return true;
    }

    public virtual CharacterControllerHelper CreateHelper() => CreateHelper(Transform.Position, Velocity, true);

    protected virtual CharacterControllerHelper CreateHelper(in Vector3 position, in Vector3 velocity, bool bounce)
    {
        return new(BuildTrace(position, position), position, velocity)
        {
            Bounce = bounce ? Bounciness : 0f,
            MaxStandableAngle = GroundAngle,
        };
    }

    protected virtual bool IsStuckAt(in Vector3 position) => BuildTrace(in position, in position).Run().StartedSolid;
    protected virtual bool IsStuck() => IsStuckAt(Transform.Position);

    public virtual bool TryUnstuck()
    {
        if(!IsStuck())
        {
            _unstuckTries = 0;
            return true;
        }

        for(int i = 0; i < 20; i++)
        {
            var offset = i == 0 ? Vector3.Up * 2f : Vector3.Random.Normal * (_unstuckTries / 2f);
            Vector3 newPosition = Transform.Position + offset;

            if(!IsStuckAt(newPosition))
            {
                _unstuckTries = 0;
                Transform.Position = newPosition;
                return true;
            }
        }

        _unstuckTries++;
        return false;
    }


    protected override void DrawGizmos()
    {
        Gizmo.GizmoDraw draw = Gizmo.Draw;
        BBox box = BoundingBox;
        draw.LineBBox(in box);
    }
}
