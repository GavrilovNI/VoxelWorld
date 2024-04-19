using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Interactions;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Worlds;
using System.Threading.Tasks;
using VoxelWorld.Mth;

namespace VoxelWorld.Players;

public class WorldInteractor : Component
{
    [Property] public Player Player { get; set; } = null!;
    [Property] public GameObject Eye { get; set; } = null!;
    [Property] public float ReachDistance => Player.ReachDistance;
    [Property] public string InteractionTag { get; set; } = "interactable";

    public SceneTraceResult TraceResult { get; protected set; }

    protected virtual SceneTraceResult Trace()
    {
        var ray = new Ray(Eye.Transform.Position, Eye.Transform.Rotation.Forward);
        return Scene.Trace.Ray(ray, ReachDistance).WithTag(InteractionTag).Run();
    }

    protected override void OnUpdate()
    {
        if(GameController.LoadingStatus != LoadingStatus.Loaded)
        {
            TraceResult = default;
            return;
        }

        TraceResult = Trace();

        bool attacking = Input.Pressed("attack1");
        bool usingItem = Input.Pressed("attack2");
        bool selectingBlock = Input.Pressed("selectblock");

        if(!attacking && !usingItem)
        {
            if(selectingBlock)
                SelectBlock(TraceResult);
            return;
        }

        bool swapInteractionOrder = Input.Down("SwapInteractionOrder");
        _ = Interact(TraceResult, attacking, swapInteractionOrder == attacking);
    }

    protected virtual InteractionResult SelectBlock(SceneTraceResult traceResult)
    {
        if(!traceResult.Hit)
            return InteractionResult.Pass;

        if(!World.TryFindInObject(traceResult.Body?.GetGameObject(), out var world))
            return InteractionResult.Pass;

        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var blockState = world.GetBlockState(blockPosition);
        var block = blockState.Block;
        if(block.IsAir())
            return InteractionResult.Pass;

        var invenory = Player.Inventory;
        var hotbar = invenory.Hotbar;

        int foundIndexInHotbar = hotbar.IndexOf(stack => !stack.IsEmpty && stack.Value is BlockItem blockItem && blockItem.Block == block);
        if(foundIndexInHotbar >= 0)
        {
            invenory.MainHandIndex = foundIndexInHotbar;
            return InteractionResult.Success;
        }

        int emptySlotIndex = invenory.MainHandIndex;
        if(!invenory.GetHandItem(HandType.Main).IsEmpty)
        {
            emptySlotIndex = hotbar.IndexOf(stack => stack.IsEmpty);
            if(emptySlotIndex < 0)
                emptySlotIndex = invenory.MainHandIndex;
        }

        if(Player.IsCreative)
        {
            if(!BlockItem.TryFind(block, out var item))
                return InteractionResult.Fail;

            var oldMainHandIndex = invenory.MainHandIndex;
            invenory.MainHandIndex = emptySlotIndex;
            bool set = invenory.TrySetHandItem(HandType.Main, new Stack<Item>(item));
            if(!set)
                invenory.MainHandIndex = oldMainHandIndex;
            return set ? InteractionResult.Success : InteractionResult.Fail;
        }

        var combinedCapability = new CombinedIndexedCapability<Stack<Item>>(invenory.Main, invenory.SecondaryHand);

        int index = 0;
        foreach(var itemStack in combinedCapability)
        {
            if(!itemStack.IsEmpty && itemStack.Value is BlockItem blockItem && blockItem.Block == block)
            {
                if(combinedCapability.TryChange(index, hotbar, emptySlotIndex, false))
                {
                    invenory.MainHandIndex = emptySlotIndex;
                    return InteractionResult.Success;
                }
            }
            ++index;
        }
        return InteractionResult.Fail;
    }

    protected virtual async Task<InteractionResult> Interact(SceneTraceResult traceResult, bool attacking, bool interactWithBlockFirst = true)
    {
        InteractionResult interactionResult;
        if(interactWithBlockFirst)
        {
            interactionResult = await InteractWithBlock(traceResult, attacking);
            if(interactionResult.ConsumesAction)
                return interactionResult;
        }

        interactionResult = await InteractWithItem(traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;

        if(!interactWithBlockFirst)
        {
            interactionResult = await InteractWithBlock(traceResult, attacking);
            if(interactionResult.ConsumesAction)
                return interactionResult;
        }

        return interactionResult;
    }


    protected virtual async Task<InteractionResult> InteractWithItem(SceneTraceResult traceResult, bool attacking)
    {
        var interactionResult = await InteractWithItem(HandType.Main, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;
        return await InteractWithItem(HandType.Secondary, traceResult, attacking);
    }

    protected virtual async Task<InteractionResult> InteractWithItem(HandType handType, SceneTraceResult traceResult, bool attacking)
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
            var itemInteractionResult = await (attacking ? item.OnAttack(itemContext) : item.OnUse(itemContext));
            if(itemInteractionResult.ConsumesAction)
                return itemInteractionResult;
        }

        return InteractionResult.Pass;
    }

    protected virtual async Task<InteractionResult> InteractWithBlock(SceneTraceResult traceResult, bool attacking)
    {
        var interactionResult = await InteractWithBlock(HandType.Main, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;
        return await InteractWithBlock(HandType.Secondary, traceResult, attacking);
    }

    protected virtual async Task<InteractionResult> InteractWithBlock(HandType handType, SceneTraceResult traceResult, bool attacking)
    {
        if(!traceResult.Hit)
            return InteractionResult.Pass;

        if(!World.TryFindInObject(traceResult.Body?.GetGameObject(), out var world))
            return InteractionResult.Pass;

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
        var blockInteractionResult = await (attacking ? block.OnAttack(blockContext) : block.OnInteract(blockContext));
        if(blockInteractionResult.ConsumesAction)
            return blockInteractionResult;

        if(attacking && !blockState.IsAir())
        {
            await blockState.Block.Break(blockContext);
            var blockCenterPosition = world.GetBlockGlobalPosition(blockPosition) + MathV.UnitsInMeter / 2f;
            Sound.Play(block.Properties.BreakSound, blockCenterPosition);
            return InteractionResult.Success;
        }

        return InteractionResult.Pass;
    }
}
