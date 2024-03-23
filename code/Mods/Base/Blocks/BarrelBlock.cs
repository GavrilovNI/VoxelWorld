using VoxelWorld.Blocks;
using VoxelWorld.Blocks.Entities;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mods.Base.Blocks.Entities;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoxelWorld.Mods.Base.Blocks;

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

    public override BlockEntity? CreateEntity(IWorldAccessor world, Vector3IntB position, BlockState blockState) =>
        BaseMod.Instance!.BlockEntities.Barrel.CreateBlockEntity(world, position);

    public override bool HasEntity(IWorldProvider world, Vector3IntB position, BlockState blockState) => true;

    public override bool IsValidEntity(IWorldProvider world, Vector3IntB position, BlockState blockState, BlockEntity blockEntity) =>
        blockEntity is BarrelBlockEntity;

}
