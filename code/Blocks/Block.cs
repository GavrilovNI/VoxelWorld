using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Blocks;

public abstract class Block : IRegisterable
{
    public ModedId ModedId { get; }
    public readonly BlockState DefaultBlockState;
    public readonly BlockStateSet BlockStateSet;

    public Block(in ModedId id)
    {
        ModedId = id;
        BlockStateSet = new BlockStateSet(this, CombineProperties());
        DefaultBlockState = CreateDefaultBlockState(BlockStateSet.First());
    }

    public virtual void OnRegistered() { }

    public virtual bool IsFullBlock(BlockState blockState) => !blockState.IsAir();
    public virtual IEnumerable<BlockProperty> CombineProperties() => Enumerable.Empty<BlockProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public bool IsAir() => this == SandcubeGame.Instance!.Blocks.Air;
    public virtual bool CanBeReplaced(BlockActionContext context, BlockState placingBlockState) => IsAir();
    public virtual BlockState GetStateForPlacement(BlockActionContext context) => DefaultBlockState;

    public virtual InteractionResult OnAttack(BlockActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnInteract(BlockActionContext context) => InteractionResult.Pass;
    public virtual void Break(BlockActionContext context) => context.World.SetBlockState(context.Position, SandcubeGame.Instance!.Blocks.Air.GetStateForPlacement(context));


    public abstract VoxelMesh CreateMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({ModedId})";

    public override int GetHashCode() => ModedId.GetHashCode();
}
