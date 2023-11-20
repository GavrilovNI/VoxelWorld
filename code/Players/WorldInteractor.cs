using Sandbox;
using Sandcube.Interactions;
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
        Item? currentItem = items.StoneSlab;
        Interact(currentItem);
    }

    protected void Interact(Item? item)
    {
        bool attacking = Input.Pressed("attack1");
        bool usingItem = Input.Pressed("attack2");

        if(!attacking && !usingItem)
            return;

        var traceResult = Trace();

        ItemActionContext itemContext = new()
        {
            Player = Player,
            Item = item!,
            TraceResult = traceResult
        };

        if(item is not null)
        {
            var itemInteractionResult = attacking ? item.OnAttack(itemContext) : item.OnUse(itemContext);
            if(itemInteractionResult.ConsumesAction)
                return;
        }

        if(!traceResult.Hit)
            return;

        var gameObject = traceResult.Body.GameObject as GameObject;
        if(gameObject is null || !gameObject.Tags.Has("world"))
            return;

        var world = Player.World;
        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var blockState = world.GetBlockState(blockPosition);

        var block = blockState.Block;
        BlockActionContext blockContext = new(itemContext);
        var blockInteractionResult = attacking ? block.OnAttack(blockContext) : block.OnInteract(blockContext);
        if(blockInteractionResult.ConsumesAction)
            return;

        if(attacking && !blockState.IsAir())
            blockState.Block.Break(blockContext);
    }
}
