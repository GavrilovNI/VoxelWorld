using VoxelWorld.Blocks.States;
using VoxelWorld.Entities;
using VoxelWorld.Interactions;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoxelWorld.Blocks;

public class PhysicsBlock : SimpleBlock
{

    public PhysicsBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    public PhysicsBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override void OnPlaced(in BlockActionContext context, BlockState placedBlockState)
    {
        if(ShouldConvertToEntity(context.World, context.Position, placedBlockState))
            ConvertToEntity(context.World, context.Position, placedBlockState);
    }

    public override void OnNeighbourChanged(in NeighbourChangedContext context)
    {
        if(ShouldConvertToEntity(context.World, context.ThisPosition, context.ThisBlockState))
            ConvertToEntity(context.World, context.ThisPosition, context.ThisBlockState);
    }

    public virtual bool ShouldConvertToEntity(IWorldAccessor world, Vector3IntB position, BlockState blockState)
    {
        var blockStateUnder = world.GetBlockState(position + Vector3IntB.Down);
        return blockStateUnder.IsAir() || blockStateUnder.Block.CanBeReplaced(blockStateUnder, blockState);
    }

    public virtual void ConvertToEntity(IWorldAccessor world, Vector3IntB position, BlockState blockState)
    {
        var realBlockState = world.GetBlockState(position);
        if(realBlockState != blockState)
            throw new System.InvalidOperationException($"{nameof(BlockState)} was {realBlockState} not {blockState}");

        world.SetBlockState(position, BlockState.Air);

        EntitySpawnConfig spawnConfig = new(new(world.GetBlockGlobalPosition(position)),
            world, false, $"{nameof(PhysicsBlockEntity)} {blockState}");

        var entity = (PhysicsBlockEntity)BaseMod.Instance!.Entities.PhysicsBlock.CreateEntity(spawnConfig);
        entity.SetBlockState(blockState);
        entity.Enabled = true;
    }
}
