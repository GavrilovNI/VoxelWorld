using Sandcube.Blocks.Entities;
using Sandcube.Registries;

namespace Sandcube.Mods.Base.Blocks.Entities;

public sealed class SandcubeBlockEntities : ModRegisterables<BlockEntityType>
{
    private static ModedId MakeId(string blockId) => new(SandcubeBaseMod.ModName, blockId);
    public BlockEntityType Barrel { get; } = new(MakeId("barrel"), () => new BarrelBlockEntity());
}
