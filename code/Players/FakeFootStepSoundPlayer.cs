using Sandbox;
using System.Linq;
using VoxelWorld.Mth;
using VoxelWorld.Players.Controllers;
using VoxelWorld.Worlds;

namespace VoxelWorld.Players;

public class FakeFootStepSoundPlayer : Component
{
    [Property] public GameObject Feet { get; set; } = null!;
    [Property] public float DistanceBetweenSteps { get; set; } = 60f;
    [Property, RequireComponent] public CharacterController CharacterController { get; set; } = null!;
    [Property, RequireComponent] public PlayerController PlayerController { get; set; } = null!;
    [Property] public float MaxTraceDownDistance { get; set; } = 100f;

    protected Vector3 LastPosition = Vector3.Zero;
    protected bool WasOnGround = false;
    protected float DistanceTravelled = 0;

    protected override void OnAwake()
    {
        Feet ??= GameObject;
    }

    protected override void OnEnabled()
    {
        LastPosition = Transform.Position;
    }

    protected override void OnUpdate()
    {
        DistanceTravelled += LastPosition.Distance(Transform.Position);
        if(CharacterController.IsOnGround && !WasOnGround ||
            DistanceTravelled > DistanceBetweenSteps)
        {
            if(!PlayerController.IsCrouching)
                PlaySound();
            DistanceTravelled = 0;
        }

        LastPosition = Transform.Position;
        WasOnGround = CharacterController.IsOnGround;
    }

    [Button("Play sound", "volume_up")]
    public virtual void PlaySoundButton() => PlaySound();

    public virtual bool PlaySound()
    {
        if(!CharacterController.GroundObject.IsValid())
            return false;

        if(!World.TryFindInObject(CharacterController.GroundObject, out var world))
            return false;

        var position = Feet.Transform.Position;
        var blockPosition = world.GetBlockPosition(position);
        var blockState = world.GetBlockState(blockPosition);

        if(blockState.IsAir())
        {
            var groundTraceResults = Scene.Trace.Ray(position, position + Transform.Rotation.Down * MaxTraceDownDistance)
                .WithAllTags(CharacterController.GroundObject.Tags).RunAll()
                .Where(r => r.GameObject.IsValid() && r.GameObject == CharacterController.GroundObject);

            if(!groundTraceResults.Any())
                return false;
            var groundTraceResult = groundTraceResults.First();

            position = groundTraceResult.EndPosition + Transform.Rotation.Down * (0.001f * MathV.UnitsInMeter);
            blockPosition = world.GetBlockPosition(position);
            blockState = world.GetBlockState(blockPosition);

            if(blockState.IsAir())
                return false;
        }

        var sound = blockState.Block.Properties.FootstepSound;
        Sound.Play(sound, position);

        return true;
    }
}
