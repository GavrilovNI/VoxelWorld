using Sandbox;
using VoxelWorld.Blocks;
using VoxelWorld.Interactions;
using VoxelWorld.Meshing;
using VoxelWorld.Mth.Enums;
using System.Threading.Tasks;

namespace VoxelWorld.Items;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block, Model model, Texture texture, int stackLimit, bool isFlatModel = false) : base(block.Id, model, texture, stackLimit, isFlatModel)
    {
        Block = block;
    }

    public BlockItem(Block block, Model model, Texture texture, bool isFlatModel = false) : base(block.Id, model, texture, isFlatModel)
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

    public static bool TryFind(Block block, out BlockItem item)
    {
        var game = GameController.Instance!;
        if(!game.Registries.TryGetRegistry<Item>(out var registry))
        {
            item = null!;
            return false;
        }

        if(registry.TryGet(block.Id, out var foundItem) &&
            foundItem is BlockItem blockItem &&
            blockItem.Block == block)
        {
            item = blockItem;
            return true;
        }

        foreach(var currentItem in registry)
        {
            if(currentItem is BlockItem currentBlockItem && currentBlockItem.Block == block)
            {
                item = currentBlockItem;
                return true;
            }
        }

        item = null!;
        return false;
    }
}
