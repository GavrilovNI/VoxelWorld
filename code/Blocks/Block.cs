using Sandcube.Blocks.Properties;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.IO;
using Sandcube.Meshing;
using Sandcube.Meshing.Blocks;
using Sandcube.Mods.Base;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Blocks;

public abstract class Block : IRegisterable, IBinaryWritable, IBinaryStaticReadable<Block>
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

    // Thread safe
    public virtual bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        return !blockState.IsAir();
    }

    // Thread safe
    public virtual bool ShouldAddFace(BlockState blockState, BlockMeshType meshType, Direction direction,
        BlockState neighborBlockState) => true;

    public virtual IEnumerable<BlockProperty> CombineProperties() => Enumerable.Empty<BlockProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public bool IsAir() => this == SandcubeBaseMod.Instance!.Blocks.Air;


    public virtual BlockState GetStateForPlacement(in BlockActionContext context) => DefaultBlockState;
    public virtual bool CanBeReplaced(BlockState currentBlockState, BlockState placingBlockState) => IsAir();
    public virtual void OnPlaced(in BlockActionContext context, BlockState placedBlockState) { }

    public virtual Task<InteractionResult> OnAttack(BlockActionContext context) => Task.FromResult(InteractionResult.Pass);
    public virtual Task<InteractionResult> OnInteract(BlockActionContext context) => Task.FromResult(InteractionResult.Pass);
    public virtual Task Break(BlockActionContext context) => Break(context.World, context.Position, context.BlockState);
    public virtual Task Break(IWorldAccessor world, Vector3Int position, BlockState blockState) => world.SetBlockState(position, BlockState.Air);

    public virtual bool CanStay(IWorldAccessor world, Vector3Int position, BlockState blockState) => true;

    public virtual void OnNeighbourChanged(in NeighbourChangedContext context)
    {
        if(!CanStay(context.World, context.ThisPosition, context.ThisBlockState))
            Break(context.World, context.ThisPosition, context.ThisBlockState);
    }


    // Thread safe
    public abstract ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({Id})";

    public override int GetHashCode() => Id.GetHashCode();

    public void Write(BinaryWriter writer) => writer.Write<ModedId>(Id);

    public static Block Read(BinaryReader reader)
    {
        var id = ModedId.Read(reader);
        var block = SandcubeGame.Instance!.Registries.GetRegistry<Block>().Get(id);
        if(block is null)
            throw new KeyNotFoundException($"Block with id {id} not found");
        return block;
    }
}
