using Sandbox;
using Sandcube.Blocks;
using Sandcube.Interactions;
using Sandcube.Mth.Enums;

namespace Sandcube.Items;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block, Texture texture) : base(block.Id, texture)
    {
        Block = block;
    }

    public override InteractionResult OnUse(in ItemActionContext context)
    {
        var traceResult = context.TraceResult;

        if(!traceResult.Hit)
            return InteractionResult.Pass;

        if(!BlockActionContext.TryMakeFromItemActionContext(context, out var blockContext))
            return InteractionResult.Pass;

        if(!TryPlace(blockContext))
        {
            blockContext += Direction.ClosestTo(blockContext.TraceResult.Normal);
            if(!TryPlace(blockContext))
                return InteractionResult.Fail;
        }

        return InteractionResult.Success;
    }

    protected virtual bool TryPlace(in BlockActionContext context)
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
