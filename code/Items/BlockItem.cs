using Sandbox;
using Sandcube.Blocks;
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

    public override InteractionResult OnUse(SandcubePlayer player, PhysicsTraceResult traceResult)
    {
        if(!traceResult.Hit)
            return InteractionResult.Pass;

        var world = SandcubeGame.Instance!.World;
        var hitBlockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var placePosition = hitBlockPosition + Direction.ClosestTo(traceResult.Normal);

        var currentBlockState = world.GetBlockState(placePosition);
        bool canReplace = currentBlockState.Block.CanBeReplaced(world, placePosition, currentBlockState, Block);
        if(!canReplace)
            return InteractionResult.Fail;

        var stateToPlace = Block.GetStateForPlacement(world, placePosition);
        world.SetBlockState(placePosition, stateToPlace);

        return InteractionResult.Success;
    }
}
