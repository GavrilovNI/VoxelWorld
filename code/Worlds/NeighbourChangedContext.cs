using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Worlds;

public record class NeighbourChangedContext
{
    public IWorldAccessor World { get; }
    public Vector3Int ThisPosition { get; }
    public BlockState ThisBlockState { get; }
    public Direction DirectionToNeighbour { get; }
    public Vector3Int NeighbourPosition { get; }
    public BlockState NeighbourOldBlockState { get; }
    public BlockState NeighbourNewBlockState { get; }

    public NeighbourChangedContext(IWorldAccessor world, Vector3Int thisPosition, BlockState thisBlockState, Direction directionToNeighbour,
        BlockState neighbourOldBlockState, BlockState neighbourNewBlockState)
    {
        World = world;
        ThisPosition = thisPosition;
        ThisBlockState = thisBlockState;
        DirectionToNeighbour = directionToNeighbour;
        NeighbourPosition = thisPosition + directionToNeighbour;
        NeighbourOldBlockState = neighbourOldBlockState;
        NeighbourNewBlockState = neighbourNewBlockState;
    }

    public NeighbourChangedContext(IWorldAccessor world, Vector3Int thisPosition, Direction directionToNeighbour,
        BlockState neighbourOldBlockState, BlockState neighbourNewBlockState) :
        this(world, thisPosition, world.GetBlockState(thisPosition), directionToNeighbour, neighbourOldBlockState, neighbourNewBlockState)
    {
    }
}
