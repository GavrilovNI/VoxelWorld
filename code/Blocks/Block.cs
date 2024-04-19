using VoxelWorld.Blocks.States;
using VoxelWorld.Blocks.States.Properties;
using VoxelWorld.Interactions;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Worlds;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace VoxelWorld.Blocks;

public abstract class Block : IRegisterable, INbtWritable, INbtStaticReadable<Block>
{
    public ModedId Id { get; }
    public readonly BlockState DefaultBlockState;
    public readonly BlockStateSet BlockStateSet;
    public BlockProperties Properties { get; init; }

    public Block(in ModedId id, in BlockProperties properties)
    {
        Id = id;
        BlockStateSet = new BlockStateSet(this, CombineProperties());
        DefaultBlockState = CreateDefaultBlockState(BlockStateSet.First());
        Properties = properties;
    }

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

    public virtual IEnumerable<BlockStateProperty> CombineProperties() => Enumerable.Empty<BlockStateProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public bool IsAir() => this == BaseMod.Instance!.Blocks.Air;


    public virtual BlockState GetStateForPlacement(in BlockActionContext context) => DefaultBlockState;
    public virtual bool CanBeReplaced(BlockState currentBlockState, BlockState placingBlockState) => IsAir();
    public virtual void OnPlaced(in BlockActionContext context, BlockState placedBlockState) { }

    public virtual async Task<InteractionResult> OnAttack(BlockAttackingContext context)
    {
        if(IsAir())
            return InteractionResult.Fail;

        if(!Properties.IsBreakable)
            return InteractionResult.Consume;

        var world = context.World;
        var blockPosition = context.Position;

        var progressDataType = BaseMod.Instance!.BlockDataTypes.BreakingProgress;
        var progress = world.GetAdditionalData(progressDataType, blockPosition);

        var progressDelta = Properties.Health <= 0f ? 1f : context.Damage / Properties.Health;
        if(progressDelta >= 0f)
        {
            progress += progressDelta;

            var blockCenterPosition = world.GetBlockGlobalPosition(blockPosition) + MathV.UnitsInMeter / 2f;
            if(progress < 1f)
                Sound.Play(Properties.DamageSound, blockCenterPosition);

            await world.SetAdditionalData(progressDataType, blockPosition, progress);

            if(progress >= 1f)
                Sound.Play(Properties.BreakSound, blockCenterPosition);

            return InteractionResult.Success;
        }

        return InteractionResult.Fail;
    }

    public virtual Task<InteractionResult> OnInteract(BlockActionContext context) => Task.FromResult(InteractionResult.Pass);
    public virtual Task Break(BlockActionContext context) => Break(context.World, context.Position, context.BlockState);
    public virtual Task Break(IWorldAccessor world, Vector3IntB position, BlockState blockState) => world.SetBlockState(position, BlockState.Air);

    public virtual bool CanStay(IWorldAccessor world, Vector3IntB position, BlockState blockState) => true;

    public virtual void OnNeighbourChanged(in NeighbourChangedContext context)
    {
        if(!CanStay(context.World, context.ThisPosition, context.ThisBlockState))
            Break(context.World, context.ThisPosition, context.ThisBlockState);
    }

    public virtual void TickRandom(IWorldAccessor world, Vector3IntB position, BlockState blockState) { }


    // Thread safe
    public abstract ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState);
    // Thread safe
    public abstract ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({Id})";

    public override int GetHashCode() => Id.GetHashCode();


    public BinaryTag Write() => Id.Write();

    public static Block Read(BinaryTag tag)
    {
        var id = ModedId.Read(tag);
        var block = GameController.Instance!.Registries.GetRegistry<Block>().Get(id);
        if(block is null)
            throw new KeyNotFoundException($"Block with id {id} not found");
        return block;
    }
}
