using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Entities;
using VoxelWorld.Items;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Worlds;
using System.Diagnostics.CodeAnalysis;

namespace VoxelWorld.Interactions;

public record class BlockActionContext
{
    public required Player Player { get; init; }
    public required Item? Item { get; init; }
    public required HandType HandType { get; init; }
    public required PhysicsTraceResult TraceResult { get; init; }

    public required IWorldAccessor World { get; init; }
    public required Vector3IntB Position { get; init; }
    public required BlockState BlockState { get; init; }

    public BlockActionContext()
    {

    }

    [SetsRequiredMembers]
    public BlockActionContext(ItemActionContext itemActionContext, IWorldAccessor world, Vector3IntB position, BlockState blockState)
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
        if(!Worlds.World.TryFindInObject(itemActionContext.TraceResult.Body?.GetGameObject(), out var world))
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
