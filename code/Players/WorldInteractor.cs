using Sandbox;
using Sandcube.Mth;
using Sandcube.Worlds;
using Sandcube.Worlds.Blocks;
using Sandcube.Worlds.Blocks.States.Properties;

namespace Sandcube.Players;

public class WorldInteractor : BaseComponent
{
    [Property] public GameObject Eye { get; set; } = null!;
    [Property] public float ReachDistance { get; set; } = 39.37f * 5;

    private PhysicsTraceResult _traceResult;
    private Vector3Int _blockPos;

    protected virtual PhysicsTraceResult TraceWorld()
    {
        var ray = new Ray(Eye.Transform.Position, Eye.Transform.Rotation.Forward);
        return Scene.PhysicsWorld.Trace.Ray(ray, ReachDistance).Run();
    }

    public override void Update()
    {
        if(!SandcubeGame.Started)
            return;

        _traceResult = TraceWorld();
        if(_traceResult.Hit)
        {
            var world = SandcubeGame.Instance!.World;
            var blocks = SandcubeGame.Instance.Blocks;
            _blockPos = world.GetBlockPosition(_traceResult.EndPosition, _traceResult.Normal);

            if(Input.Pressed("attack1"))
            {
                world.SetBlockState(_blockPos, blocks.Air.DefaultBlockState);
            }
            else if(Input.Pressed("attack2"))
            {
                var setPosition = _blockPos + Direction.ClosestTo(_traceResult.Normal);
                if(!world.GetBlockState(setPosition).IsAir())
                    return;

                var blockState = Game.Random.Next(100) < 50 ? blocks.StoneSlab.DefaultBlockState : blocks.StoneSlab.DefaultBlockState.With(SlabBlock.SlabTypeProperty, (Enum<SlabBlock.SlabType>)SlabBlock.SlabType.Top);
                world.SetBlockState(setPosition, blockState);
            }
        }
    }

    public override void DrawGizmos()
    {
        if(!SandcubeGame.Started)
            return;

        using var scope = Gizmo.Scope($"{GetHashCode()}");
        Gizmo.Transform = Gizmo.Transform.ToLocal(Gizmo.Transform);
        Gizmo.Draw.Color = (_traceResult.Hit ? Color.Green : Color.Red).WithAlpha(0.4f);
        Gizmo.Draw.Line(_traceResult.StartPosition, _traceResult.EndPosition);

        if(_traceResult.Hit)
        {
            var world = SandcubeGame.Instance!.World;
            var blockWoorldPOsition = world.GetBlockWorldPosition(_blockPos);
            Gizmo.Draw.Color = Color.White.WithAlpha(0.4f);
            Gizmo.Draw.LineBBox(new BBox(blockWoorldPOsition, blockWoorldPOsition + world.VoxelSize));
        }
    }
}
