using Sandbox;
using Sandcube.Items;

namespace Sandcube.Players;

public class WorldInteractor : BaseComponent
{
    [Property] public SandcubePlayer Player { get; set; } = null!;
    [Property] public GameObject Eye { get; set; } = null!;
    [Property] public float ReachDistance { get; set; } = 39.37f * 5;

    protected virtual PhysicsTraceResult Trace()
    {
        var ray = new Ray(Eye.Transform.Position, Eye.Transform.Rotation.Forward);
        return Scene.PhysicsWorld.Trace.Ray(ray, ReachDistance).Run();
    }

    public override void Update()
    {
        if(!SandcubeGame.IsStarted)
            return;

        var items = SandcubeGame.Instance!.Items;
        Item? currentItem = items.Dirt;
        Interact(currentItem);
    }

    protected void Interact(Item? item)
    {
        bool attacking = Input.Pressed("attack1");
        bool usingItem = Input.Pressed("attack2");

        if(!attacking && !usingItem)
            return;

        var traceResult = Trace();

        if(item is not null)
        {
            var itemInteractionResult = attacking ? item.OnAttack(Player, traceResult) : item.OnUse(Player, traceResult);
            if(itemInteractionResult.ConsumesAction)
                return;
        }

        var world = Player.World;
        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var blockState = world.GetBlockState(blockPosition);

        var block = blockState.Block;
        var blockInteractionResult = attacking ? block.OnAttack(world, blockPosition, blockState, Player) :
            block.OnInteract(world, blockPosition, blockState, Player);
        if(blockInteractionResult.ConsumesAction)
            return;

        if(!blockState.IsAir())
            world.SetBlockState(blockPosition, SandcubeGame.Instance!.Blocks.Air.GetStateForPlacement(world, blockPosition));
    }
}
