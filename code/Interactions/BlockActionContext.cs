using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Items;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Worlds;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Interactions;

public record class BlockActionContext
{
    public required SandcubePlayer Player { get; init; }
    public required Item? Item { get; init; }
    public required PhysicsTraceResult TraceResult { get; init; }

    public required IWorldAccessor World { get; init; }
    public required Vector3Int Position { get; init; }
    public required BlockState BlockState { get; init; }

    public BlockActionContext()
    {

    }

    [SetsRequiredMembers]
    public BlockActionContext(ItemActionContext itemActionContext)
    {
        Player = itemActionContext.Player;
        Item = itemActionContext.Item;
        TraceResult = itemActionContext.TraceResult;

        World = Player.World;
        Position = World.GetBlockPosition(TraceResult.EndPosition, TraceResult.Normal);
        BlockState = World.GetBlockState(Position);
    }

    [SetsRequiredMembers]
    public BlockActionContext(ItemActionContext itemActionContext, Vector3Int position)
    {
        Player = itemActionContext.Player;
        Item = itemActionContext.Item;
        TraceResult = itemActionContext.TraceResult;

        World = Player.World;
        Position = position;
        BlockState = World.GetBlockState(Position);
    }

    [SetsRequiredMembers]
    public BlockActionContext(ItemActionContext itemActionContext, IWorldAccessor world, Vector3Int position, BlockState blockState)
    {
        Player = itemActionContext.Player;
        Item = itemActionContext.Item;
        TraceResult = itemActionContext.TraceResult;

        World = world;
        Position = position;
        BlockState = blockState;
    }
}
