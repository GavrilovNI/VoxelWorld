using VoxelWorld.Blocks.Entities;

namespace VoxelWorld.Mods.Base.Blocks.Entities;

public sealed class BarrelBlockEntity : ItemStorageBlockEntity
{
    public BarrelBlockEntity() : base(BaseMod.Instance!.BlockEntities.Barrel, 27)
    {
    }
}
