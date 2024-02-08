﻿using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Items;
using Sandcube.Worlds;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Players;

public class WorldInteractor : Component
{
    [Property] public SandcubePlayer Player { get; set; } = null!;
    [Property] public GameObject Eye { get; set; } = null!;
    [Property] public float ReachDistance { get; set; } = 39.37f * 5;

    [Property] public string InteractionTag { get; set; } = "interactable";

    protected virtual PhysicsTraceResult Trace()
    {
        var ray = new Ray(Eye.Transform.Position, Eye.Transform.Rotation.Forward);
        return Scene.PhysicsWorld.Trace.Ray(ray, ReachDistance).WithTag(InteractionTag).Run();
    }

    protected override void OnUpdate()
    {
        if(!SandcubeGame.IsStarted)
            return;

        bool attacking = Input.Pressed("attack1");
        bool usingItem = Input.Pressed("attack2");
        bool selectingBlock = Input.Pressed("selectblock");

        if(!attacking && !usingItem)
        {
            if(selectingBlock)
                SelectBlock();
            return;
        }

        bool swapInteractionOrder = Input.Down("SwapInteractionOrder");
        Interact(attacking, !swapInteractionOrder);
    }

    protected virtual InteractionResult SelectBlock()
    {
        var traceResult = Trace();

        if(!traceResult.Hit)
            return InteractionResult.Pass;

        IWorldAccessor? world = traceResult.Body?.GetGameObject()?.Components?.Get<IWorldAccessor>();
        if(world is null)
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
            var item = SandcubeGame.Instance!.ItemsRegistry.FirstOrDefault(i => i is BlockItem blockItem && blockItem.Block == block, null);
            if(item is null)
                return InteractionResult.Fail;

            var oldMainHandIndex = invenory.MainHandIndex;
            invenory.MainHandIndex = emptySlotIndex;
            bool set = invenory.TrySetHandItem(HandType.Main, new Inventories.Stack<Item>(item));
            if(!set)
                invenory.MainHandIndex = oldMainHandIndex;
            return set ? InteractionResult.Success : InteractionResult.Fail;
        }

        var combinedCapability = new CombinedIndexedCapability<Inventories.Stack<Item>>(invenory.Main, invenory.SecondaryHand);

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

    protected virtual InteractionResult Interact(bool attacking, bool interactWithBlockFirst = true)
    {
        var traceResult = Trace();

        InteractionResult interactionResult;
        if(interactWithBlockFirst)
        {
            interactionResult = InteractWithBlock(traceResult, attacking);
            if(interactionResult.ConsumesAction)
                return interactionResult;
        }

        interactionResult = InteractWithItem(traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;

        if(!interactWithBlockFirst)
        {
            interactionResult = InteractWithBlock(traceResult, attacking);
            if(interactionResult.ConsumesAction)
                return interactionResult;
        }

        return interactionResult;
    }


    protected virtual InteractionResult InteractWithItem(PhysicsTraceResult traceResult, bool attacking)
    {
        var interactionResult = InteractWithItem(HandType.Main, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;
        return InteractWithItem(HandType.Secondary, traceResult, attacking);
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

    protected virtual InteractionResult InteractWithBlock(PhysicsTraceResult traceResult, bool attacking)
    {
        var interactionResult = InteractWithBlock(HandType.Main, traceResult, attacking);
        if(interactionResult.ConsumesAction)
            return interactionResult;
        return InteractWithBlock(HandType.Secondary, traceResult, attacking);
    }

    protected virtual InteractionResult InteractWithBlock(HandType handType, PhysicsTraceResult traceResult, bool attacking)
    {
        if(!traceResult.Hit)
            return InteractionResult.Pass;

        IWorldAccessor? world = traceResult.Body?.GetGameObject()?.Components?.Get<IWorldAccessor>();
        if(world is null)
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
