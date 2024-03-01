using Sandbox;
using Sandcube.Blocks;
using Sandcube.Interactions;
using Sandcube.Mth.Enums;
using System.Threading.Tasks;

namespace Sandcube.Items;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block, Texture texture) : base(block.Id, texture)
    {
        Block = block;
    }

    public override async Task<InteractionResult> OnUse(ItemActionContext context)
    {
        var traceResult = context.TraceResult;

        if(!traceResult.Hit)
            return InteractionResult.Pass;

        if(!BlockActionContext.TryMakeFromItemActionContext(context, out var blockContext))
            return InteractionResult.Pass;

        if(!await TryPlace(blockContext))
        {
            blockContext += Direction.ClosestTo(blockContext.TraceResult.Normal);
            if(!await TryPlace(blockContext))
                return InteractionResult.Fail;
        }

        return InteractionResult.Success;
    }

    protected virtual async Task<bool> TryPlace(BlockActionContext context)
    {
        var currentBlockState = context.BlockState;

        var stateToPlace = Block.GetStateForPlacement(context);
        var block = stateToPlace.Block;

        bool canReplace = currentBlockState.Block.CanBeReplaced(context.BlockState, stateToPlace);
        if(!canReplace)
            return false;

        bool canStay = block.CanStay(context.World, context.Position, stateToPlace);
        if(!canStay)
            return false;

        var contextCopy = context;

        var changed = await context.World.SetBlockState(context.Position, stateToPlace);
        if(changed)
            block.OnPlaced(contextCopy, stateToPlace);

        return changed;
    }
}
