using VoxelWorld.Blocks.States;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VoxelWorld.Blocks;

public class SimpleBlock : Block
{
    public IReadOnlyDictionary<Direction, IUvProvider> UvProviders { get; private set; }


    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, IUvProvider uvProvider) : base(id)
    {
        UvProviders = new Dictionary<Direction, IUvProvider>()
        {
            { Direction.Forward, uvProvider },
            { Direction.Backward, uvProvider },
            { Direction.Left, uvProvider },
            { Direction.Right, uvProvider },
            { Direction.Up, uvProvider },
            { Direction.Down, uvProvider }
        };
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id)
    {
        UvProviders = uvProviders;
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        return VisualMeshes.FullBlock.Make(UvProviders.ToDictionary(p => p.Key, p => p.Value.Uv));
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
