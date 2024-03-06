using Sandcube.Blocks.Entities;

namespace Sandcube.Mods.Base.Blocks.Entities;

public sealed class BarrelBlockEntity : ItemStorageBlockEntity
{
    public BarrelBlockEntity() : base(SandcubeBaseMod.Instance!.BlockEntities.Barrel, 27)
    {
    }
}
