using Sandcube.Blocks.Entities;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
using Sandcube.Interactions;
using Sandcube.Menus;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sandcube.Blocks;

public abstract class ItemStorageBlock : SimpleBlock, IEntityBlock
{
    [SetsRequiredMembers]
    public ItemStorageBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    [SetsRequiredMembers]
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

    public abstract BlockEntity? CreateEntity(IWorldAccessor world, Vector3Int position, BlockState blockState);
    public abstract bool HasEntity(IWorldProvider world, Vector3Int position, BlockState blockState);
    public abstract bool IsValidEntity(IWorldProvider world, Vector3Int position, BlockState blockState, BlockEntity blockEntity);
}
