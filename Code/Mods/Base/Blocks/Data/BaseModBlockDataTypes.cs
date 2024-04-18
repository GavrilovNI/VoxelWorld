using VoxelWorld.Registries;
using VoxelWorld.Worlds.Data;

namespace VoxelWorld.Mods.Base.Blocks.Data;

public class BaseModBlockDataTypes : ModRegisterables<BlocksAdditionalDataType>
{
    private static ModedId MakeId(string blockId) => new(BaseMod.ModName, blockId);


    public BlockBreakingProgressDataType BreakingProgress { get; private set; } = new(MakeId("breaking_progress"));
}
