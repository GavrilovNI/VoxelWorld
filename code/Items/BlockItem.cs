﻿using Sandbox;
using Sandcube.Blocks;
using Sandcube.Interactions;
using Sandcube.Mth.Enums;
using Sandcube.Players;

namespace Sandcube.Items;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block) : base(block.ModedId)
    {
        Block = block;
    }

    public override InteractionResult OnUse(ItemActionContext context)
    {
        var traceResult = context.TraceResult;

        if(!traceResult.Hit)
            return InteractionResult.Pass;

        var gameObject = traceResult.Body.GameObject as GameObject;
        if(gameObject is null || !gameObject.Tags.Has("world"))
            return InteractionResult.Pass;

        BlockActionContext blockContext = new(context);

        if(!TryPlace(blockContext))
        {
            blockContext = new(context, blockContext.Position + Direction.ClosestTo(blockContext.TraceResult.Normal));

            if(!TryPlace(blockContext))
                return InteractionResult.Fail;
        }

        return InteractionResult.Success;
    }

    protected virtual bool TryPlace(BlockActionContext context)
    {
        var currentBlockState = context.BlockState;

        var stateToPlace = Block.GetStateForPlacement(context);
        bool canReplace = currentBlockState.Block.CanBeReplaced(context, stateToPlace);
        if(!canReplace)
            return false;

        context.World.SetBlockState(context.Position, stateToPlace);
        return true;
    }
}
