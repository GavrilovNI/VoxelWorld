using Sandcube.Mods.Base;

namespace Sandcube.Blocks.Entities;

public sealed class BarrelBlockEntity : ItemStorageBlockEntity
{
    public BarrelBlockEntity() : base(SandcubeBaseMod.Instance!.BlockEntities.Barrel, 27)
    {
    }
}
