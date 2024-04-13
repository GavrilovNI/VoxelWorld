using VoxelWorld.Blocks.Entities;
using VoxelWorld.Blocks.Interfaces;
using VoxelWorld.Blocks.States;
using VoxelWorld.Interactions;
using VoxelWorld.Menus;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace VoxelWorld.Blocks;

public abstract class ItemStorageBlock : SimpleBlock, IEntityBlock
{
    public ItemStorageBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    public ItemStorageBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override Task<InteractionResult> OnInteract(BlockActionContext context)
    {
        if(context.World.GetBlockEntity(context.Position) is ItemStorageBlockEntity blockEntity)
        {
            var menu = blockEntity.CreateMenu(context.Player);
            MenuController.Instance!.Open(menu);
            return Task.FromResult(InteractionResult.Success);
        }
        return Task.FromResult(InteractionResult.Fail);
    }

    public abstract BlockEntity? CreateEntity(IWorldAccessor world, Vector3IntB position, BlockState blockState);
    public abstract bool HasEntity(IWorldProvider world, Vector3IntB position, BlockState blockState);
    public abstract bool IsValidEntity(IWorldProvider world, Vector3IntB position, BlockState blockState, BlockEntity blockEntity);
}
