using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Interactions;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Worlds;
using System.Threading.Tasks;
using VoxelWorld.Controlling;
using VoxelWorld.Mods.Base;
using System;
using System.Linq;
using System.Collections.Generic;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;

namespace VoxelWorld.Players;

using ItemStack = Inventories.Stack<Item>;

public class WorldInteractor : Component
{
    [Property] public Player Player { get; set; } = null!;
    [Property] public GameObject Eye { get; set; } = null!;
    [Property] public float ReachDistance => Player.ReachDistance;
    [Property] public string InteractionTag { get; set; } = "interactable";


    [Property] public ItemProperties HandProperties { get; set; } = new();
    public float AttackingTime { get; protected set; } = 0f;
    public TimeUntil TimeUntilAttack { get; protected set; } = 0f;

    public SceneTraceResult TraceResult { get; protected set; }

    protected bool IsUpdating = false;

    protected delegate Task<InteractionResult> HandFunction(SceneTraceResult traceResult, HandType hand, ItemStack handItem);


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

        if(GameInput.IsSelectingBlockPressed)
            SelectBlock(TraceResult);

        if(!IsUpdating)
            _ = UpdateAsync();
    }

    protected virtual async Task UpdateAsync()
    {
        if(IsUpdating)
            return;
        IsUpdating = true;

        bool isAttacking = Player.IsCreative ? GameInput.IsAttackingPressed : GameInput.IsAttacking;
        if(isAttacking)
        {
            var interactionResult = await HandleAttacking();
            if(interactionResult.ConsumesAction)
            {
                IsUpdating = false;
                return;
            }
        }

        bool isInteracting = GameInput.IsInteractPressed;
        if(isInteracting)
            await Interact(TraceResult, !GameInput.IsSwappingInteractionOrder);

        IsUpdating = false;
    }


    protected virtual Task<InteractionResult> HandleAttacking()
    {
        if(TimeUntilAttack > 0)
            return Task.FromResult(InteractionResult.Pass);

        return Attack(TraceResult);
    }

    protected virtual async Task<InteractionResult> Attack(SceneTraceResult traceResult)
    {
        var (attackingHand, attackingStack) = GetNotEmptyHandOrMain();

        ItemProperties itemProperties;

        if(attackingStack.IsEmpty)
        {
            itemProperties = HandProperties;
        }
        else
        {
            var attackingItem = attackingStack.Value!;
            itemProperties = attackingItem.Properties;

            ItemActionContext itemContext = new()
            {
                Player = Player,
                Stack = attackingStack,
                HandType = attackingHand,
                TraceResult = traceResult
            };

            var interactionResult = await attackingItem.OnAttack(itemContext);
            if(interactionResult.ConsumesAction)
                return interactionResult;

        }

        AttackingTime = itemProperties.AttackingTime;
        TimeUntilAttack = AttackingTime;

        if(!traceResult.Hit)
            return InteractionResult.Consume;

        var attackedGameObject = traceResult.Body?.GetGameObject();
        if(!attackedGameObject.IsValid())
            return InteractionResult.Consume;

        var damageable = attackedGameObject.Components.Get<IDamageable>();
        if(damageable is not null)
        {
            damageable.OnDamage(new EntityDamageInfo(itemProperties.Damage, Player, attackingStack));
            return InteractionResult.Success;
        }

        if(World.TryFindInObject(attackedGameObject, out var world))
        {
            var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
            if(!world.IsLoadedAt(blockPosition))
                return InteractionResult.Consume;

            var blockState = world.GetBlockState(blockPosition);
            var block = blockState.Block;

            if(block.IsAir())
                return InteractionResult.Consume;

            BlockAttackingContext blockContext = new()
            {
                Player = Player,
                Stack = attackingStack,
                HandType = attackingHand,
                TraceResult = traceResult,
                World = world,
                Position = blockPosition,
                BlockState = blockState,
                Damage = block.Properties.IsBreakable ? itemProperties.BlockDamage : 0f,
            };

            var interactionResult = await block.OnAttack(blockContext);

            if(Player.IsCreative)
            {
                var changed = await world.SetBlockState(blockPosition, BlockState.Air);
                if(changed)
                {
                    var blockCenterPosition = world.GetBlockGlobalPosition(blockPosition) + MathV.UnitsInMeter / 2f;
                    Sound.Play(block.Properties.BreakSound, blockCenterPosition);
                    return InteractionResult.Success;
                }
            }

            return interactionResult;
        }

        return InteractionResult.Consume;
    }

    protected (HandType HandType, ItemStack Stack) GetNotEmptyHandOrMain()
    {
        ItemStack mainHandStack = Player.Inventory.GetHandItem(HandType.Main);

        if(mainHandStack.IsEmpty)
        {
            var secondaryHandStack = Player.Inventory.GetHandItem(HandType.Secondary);
            if(!secondaryHandStack.IsEmpty)
                return (HandType.Secondary, secondaryHandStack);
        }

        return (HandType.Main, mainHandStack);
    }


    protected virtual async Task<InteractionResult> Interact(SceneTraceResult traceResult, bool interactWithBlockFirst)
    {
        IEnumerable<HandFunction> functions = new HandFunction[] { InteractWithBlock, UseItem };

        if(!interactWithBlockFirst)
            functions = functions.Reverse();

        InteractionResult interactionResult = InteractionResult.Pass;
        foreach(var function in functions)
        {
            interactionResult = await DoForBothHands(traceResult, function);
            if(interactionResult.ConsumesAction)
                return interactionResult;
        }

        return interactionResult;
    }

    protected virtual async Task<InteractionResult> DoForBothHands(SceneTraceResult traceResult, HandFunction function)
    {
        var (hand, stack) = GetNotEmptyHandOrMain();

        var interactionResult = await function(traceResult, hand, stack);
        if(interactionResult.ConsumesAction)
            return interactionResult;

        hand = hand.GetOpposite();
        stack = Player.Inventory.GetHandItem(hand);
        return await function(traceResult, hand, stack);
    }

    protected virtual Task<InteractionResult> InteractWithBlock(SceneTraceResult traceResult, HandType hand, ItemStack handStack)
    {
        if(!traceResult.Hit)
            return Task.FromResult(InteractionResult.Fail);

        if(!World.TryFindInObject(traceResult.Body?.GetGameObject(), out var world))
            return Task.FromResult(InteractionResult.Fail);

        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);

        if(!world.IsLoadedAt(blockPosition))
            return Task.FromResult(InteractionResult.Fail);

        var blockState = world.GetBlockState(blockPosition);
        var block = blockState.Block;

        BlockActionContext blockContext = new()
        {
            Player = Player,
            Stack = handStack,
            HandType = hand,
            TraceResult = traceResult,
            World = world,
            Position = blockPosition,
            BlockState = blockState
        };

        return block.OnInteract(blockContext);
    }

    protected virtual Task<InteractionResult> UseItem(SceneTraceResult traceResult, HandType hand, ItemStack handStack)
    {
        if(handStack.IsEmpty)
            return Task.FromResult(InteractionResult.Pass);

        ItemActionContext itemContext = new()
        {
            Player = Player,
            Stack = handStack,
            HandType = hand,
            TraceResult = traceResult,
        };

        return handStack.Value!.OnUse(itemContext);
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
            bool set = invenory.TrySetHandItem(HandType.Main, new ItemStack(item));
            if(!set)
                invenory.MainHandIndex = oldMainHandIndex;
            return set ? InteractionResult.Success : InteractionResult.Fail;
        }

        var combinedCapability = new CombinedIndexedCapability<ItemStack>(invenory.Main, invenory.SecondaryHand);

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

}
