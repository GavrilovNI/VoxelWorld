using Sandcube.Blocks;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mods.Base;
using Sandcube.Mods.Base.Blocks.Entities;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Mods.Base.Blocks;

public class BarrelBlock : ItemStorageBlock
{
    [SetsRequiredMembers]
    public BarrelBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    [SetsRequiredMembers]
    public BarrelBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override BlockEntity? CreateEntity(IWorldAccessor world, Vector3Int position, BlockState blockState) =>
        SandcubeBaseMod.Instance!.BlockEntities.Barrel.CreateBlockEntity(world, position);

    public override bool HasEntity(IWorldProvider world, Vector3Int position, BlockState blockState) => true;

    public override bool IsValidEntity(IWorldProvider world, Vector3Int position, BlockState blockState, BlockEntity blockEntity) =>
        blockEntity is BarrelBlockEntity;

}
