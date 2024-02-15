using Sandcube.Blocks.States;
using Sandcube.Meshing;
using Sandcube.Meshing.Blocks;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class XShapedBlock : Block
{
    public IUvProvider UvProvider { get; }

    [SetsRequiredMembers]
    public XShapedBlock(in ModedId id, IUvProvider uvProvider) : base(id)
    {
        UvProvider = uvProvider;
    }

    public override bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace) => false;


    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState) => VisualMeshes.XShaped(UvProvider.Uv);
    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.Empty;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
