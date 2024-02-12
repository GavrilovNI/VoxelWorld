using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Items;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Players;
using Sandcube.Worlds;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Interactions;

public record class BlockActionContext
{
    public required SandcubePlayer Player { get; init; }
    public required Item? Item { get; init; }
    public required HandType HandType { get; init; }
    public required PhysicsTraceResult TraceResult { get; init; }

    public required IWorldAccessor World { get; init; }
    public required Vector3Int Position { get; init; }
    public required BlockState BlockState { get; init; }

    public BlockActionContext()
    {

    }

    [SetsRequiredMembers]
    public BlockActionContext(ItemActionContext itemActionContext, IWorldAccessor world, Vector3Int position, BlockState blockState)
    {
        Player = itemActionContext.Player;
        Item = itemActionContext.Item;
        HandType = itemActionContext.HandType;
        TraceResult = itemActionContext.TraceResult;

        World = world;
        Position = position;
        BlockState = blockState;
    }

    public static bool TryMakeFromItemActionContext(ItemActionContext itemActionContext, out BlockActionContext context)
    {
        var world = itemActionContext.TraceResult.Body?.GetGameObject()?.Components?.Get<IWorldAccessor>();
        if(world == null)
        {
            context = null!;
            return false;
        }

        var position = world.GetBlockPosition(itemActionContext.TraceResult.EndPosition, itemActionContext.TraceResult.Normal);
        var blockState = world.GetBlockState(position);
        context = new BlockActionContext(itemActionContext, world, position, blockState);
        return true;
    }

    public static BlockActionContext operator +(in BlockActionContext context, Direction direction)
    {
        var newPosition = context.Position + direction;
        return context with
        {
            Position = newPosition,
            BlockState = context.World.GetBlockState(newPosition)
        };
    }
}
