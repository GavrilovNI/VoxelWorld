using System.Threading.Tasks;
using VoxelWorld.Mth;
using VoxelWorld.Registries;
using VoxelWorld.Worlds;
using VoxelWorld.Worlds.Data;

namespace VoxelWorld.Mods.Base.Blocks.Data;

public class BlockBreakingProgressDataType : BlocksAdditionalDataType<BlockBreakingProgress>
{
    public BlockBreakingProgressDataType(ModedId id) : base(id, new())
    {
    }

    public override Task OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, in BlockBreakingProgress newProgress)
    {
        if(newProgress >= 1f)
        {
            var blockState = world.GetBlockState(blockPosition);
            return blockState.Block.Break(world, blockPosition, blockState);
        }
        return Task.CompletedTask;
    }
}
