using VoxelWorld.Blocks.Entities;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;

namespace VoxelWorld.Blocks.Interfaces;

public interface IEntityBlock
{
    public bool HasEntity(IWorldProvider world, Vector3Int position, BlockState blockState);
    public BlockEntity? CreateEntity(IWorldAccessor world, Vector3Int position, BlockState blockState);
    public bool IsValidEntity(IWorldProvider world, Vector3Int position, BlockState blockState, BlockEntity blockEntity);
}
