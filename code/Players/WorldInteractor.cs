using Sandbox;
using Sandcube.Blocks.States;
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

        bool attacking = Input.Pressed("attack1");
        bool usingItem = Input.Pressed("attack2");

        if(!attacking && !usingItem)
            return;

        Interact(attacking);
    }

    protected virtual InteractionResult Interact(bool attacking)
    {
        var traceResult = Trace();

        var interactionResult = InteractWithItem(HandType.Main, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;

        interactionResult = InteractWithItem(HandType.Secondary, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;

        return InteractWithBlock(HandType.Secondary, traceResult, attacking);
    }

    protected virtual InteractionResult InteractWithItem(HandType handType, PhysicsTraceResult traceResult, bool attacking)
    {
        Item? item = Player.Inventory.GetHandItem(handType).Value;

        ItemActionContext itemContext = new()
        {
            Player = Player,
            Item = item!,
            HandType = handType,
            TraceResult = traceResult
        };

        if(item is not null)
        {
            var itemInteractionResult = attacking ? item.OnAttack(itemContext) : item.OnUse(itemContext);
            if(itemInteractionResult.ConsumesAction)
                return itemInteractionResult;
        }

        return InteractionResult.Pass;
    }

    protected virtual InteractionResult InteractWithBlock(HandType handType, PhysicsTraceResult traceResult, bool attacking)
    {
        if(!traceResult.Hit)
            return InteractionResult.Pass;

        var gameObject = traceResult.Body.GameObject as GameObject;
        if(gameObject is null || !gameObject.Tags.Has("world"))
            return InteractionResult.Pass;

        var world = Player.World;
        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var blockState = world.GetBlockState(blockPosition);

        var block = blockState.Block;

        Item? item = Player.Inventory.GetHandItem(handType).Value;
        BlockActionContext blockContext = new()
        {
            Player = Player,
            Item = item,
            HandType = handType,
            TraceResult = traceResult,
            World = world,
            Position = blockPosition,
            BlockState = blockState
        };
        var blockInteractionResult = attacking ? block.OnAttack(blockContext) : block.OnInteract(blockContext);
        if(blockInteractionResult.ConsumesAction)
            return blockInteractionResult;


        if(attacking && !blockState.IsAir())
        {
            blockState.Block.Break(blockContext);
            return InteractionResult.Success;
        }

        return InteractionResult.Pass;
    }
}
