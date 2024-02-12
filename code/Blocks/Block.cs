using Sandcube.Blocks.Properties;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mods;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing.Items;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation.Meshes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sandcube.Blocks;

[CustomBlockItemTextureMakerAttribute(typeof(BlockItemTextureMaker))]
public abstract class Block : IRegisterable
{
    public ModedId Id { get; }
    public readonly BlockState DefaultBlockState;
    public readonly BlockStateSet BlockStateSet;
    public required BlockProperties Properties { get; init; }

    [SetsRequiredMembers]
    public Block(in ModedId id, in BlockProperties properties)
    {
        Id = id;
        BlockStateSet = new BlockStateSet(this, CombineProperties());
        DefaultBlockState = CreateDefaultBlockState(BlockStateSet.First());
        Properties = properties;
    }

    [SetsRequiredMembers]
    public Block(in ModedId id) : this(id, BlockProperties.Default)
    {
    }

    public virtual void OnRegistered() { }

    // Thread safe
    public virtual bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        return !blockState.IsAir();
    }

    public virtual IEnumerable<BlockProperty> CombineProperties() => Enumerable.Empty<BlockProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public bool IsAir() => this == SandcubeBaseMod.Instance!.Blocks.Air;


    public virtual BlockState GetStateForPlacement(in BlockActionContext context) => DefaultBlockState;
    public virtual bool CanBeReplaced(in BlockActionContext context, BlockState placingBlockState) => IsAir();
    public virtual void OnPlaced(in BlockActionContext context, BlockState placedBlockState) { }

    public virtual InteractionResult OnAttack(in BlockActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnInteract(in BlockActionContext context) => InteractionResult.Pass;
    public virtual void Break(in BlockActionContext context) => context.World.SetBlockState(context.Position, BlockState.Air);

    public virtual bool CanStay(IWorldAccessor world, Vector3Int position, BlockState blockState) => true;

    public virtual void OnNeighbourChanged(in NeighbourChangedContext context) { }


    // Thread safe
    public abstract ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({Id})";

    public override int GetHashCode() => Id.GetHashCode();
}
