using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Items;
using Sandcube.Menus;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class ItemStorageBlock : SimpleBlock, IEntityBlock
{
    public readonly int StorageSize;
    public readonly int StorageStackLimit;

    [SetsRequiredMembers]
    public ItemStorageBlock(in ModedId id, IUvProvider uvProvider,
        int storageSize, int storageStackLimit = DefaultValues.ItemStackLimit) : base(id, uvProvider)
    {
        if(storageSize < 0)
            throw new ArgumentOutOfRangeException(nameof(storageSize));
        if(storageStackLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(storageStackLimit));
        StorageSize = storageSize;
        StorageStackLimit = storageStackLimit;
    }

    [SetsRequiredMembers]
    public ItemStorageBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders,
        int storageSize, int storageStackLimit = DefaultValues.ItemStackLimit) : base(id, uvProviders)
    {
        if(storageSize < 0)
            throw new ArgumentOutOfRangeException(nameof(storageSize));
        if(storageStackLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(storageStackLimit));
        StorageSize = storageSize;
        StorageStackLimit = storageStackLimit;
    }

    public override InteractionResult OnInteract(BlockActionContext context)
    {
        if(context.World.GetBlockEntity(context.Position) is ItemStorageBlockEntity blockEntity)
        {
            var menu = blockEntity.CreateMenu(context.Player);
            MenuController.Instance!.Open(menu);
            return InteractionResult.Success;
        }
        return InteractionResult.Fail;
    }

    public BlockEntity? CreateEntity(IWorldProvider world, Vector3Int position, BlockState blockState) => new ItemStorageBlockEntity(world, position, StorageSize, StorageStackLimit);
    public bool HasEntity(IWorldProvider world, Vector3Int position, BlockState blockState) => true;
}
