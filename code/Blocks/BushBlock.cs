using VoxelWorld.Blocks.States;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System.Diagnostics.CodeAnalysis;

namespace VoxelWorld.Blocks;

public class BushBlock : Block
{
    public IUvProvider UvProvider { get; }

    [SetsRequiredMembers]
    public BushBlock(in ModedId id, IUvProvider uvProvider) : base(id)
    {
        UvProvider = uvProvider;
    }

    public override bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace) => false;


    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState) => VisualMeshes.XShaped(UvProvider.Uv);
    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.Empty;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;

    protected virtual bool CanStayOn(BlockState blockState, BlockState blockStateUnder)
    {
        var blocks = BaseMod.Instance!.Blocks;

        var blockUnder = blockStateUnder.Block;
        return blockUnder == blocks.Dirt ||
            blockUnder == blocks.Grass;
    }

    public override bool CanStay(IWorldAccessor world, Vector3Int position, BlockState blockState)
    {
        var blockStateUnder = world.GetBlockState(position + Vector3Int.Down);
        return CanStayOn(blockState, blockStateUnder);
    }
}
