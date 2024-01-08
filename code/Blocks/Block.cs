using Sandcube.Blocks.Properties;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds.Generation.Meshes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sandcube.Blocks;

public abstract class Block : IRegisterable
{
    public ModedId ModedId { get; }
    public readonly BlockState DefaultBlockState;
    public readonly BlockStateSet BlockStateSet;
    public required BlockProperties Properties { get; init; }

    [SetsRequiredMembers]
    public Block(in ModedId id, in BlockProperties properties)
    {
        ModedId = id;
        BlockStateSet = new BlockStateSet(this, CombineProperties());
        DefaultBlockState = CreateDefaultBlockState(BlockStateSet.First());
        Properties = properties;
    }

    [SetsRequiredMembers]
    public Block(in ModedId id) : this(id, BlockProperties.Default)
    {
    }

    public virtual void OnRegistered() { }

    public virtual bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        return !blockState.IsAir();
    }

    public virtual IEnumerable<BlockProperty> CombineProperties() => Enumerable.Empty<BlockProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public bool IsAir() => this == SandcubeGame.Instance!.Blocks.Air;
    public virtual bool CanBeReplaced(BlockActionContext context, BlockState placingBlockState) => IsAir();
    public virtual BlockState GetStateForPlacement(BlockActionContext context) => DefaultBlockState;

    public virtual InteractionResult OnAttack(BlockActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnInteract(BlockActionContext context) => InteractionResult.Pass;
    public virtual void Break(BlockActionContext context) => context.World.SetBlockState(context.Position, BlockState.Air);


    public abstract ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState);
    public abstract ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState);
    public abstract ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({ModedId})";

    public override int GetHashCode() => ModedId.GetHashCode();
}
