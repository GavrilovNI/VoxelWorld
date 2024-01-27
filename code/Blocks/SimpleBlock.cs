using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds.Generation.Meshes;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class SimpleBlock : Block
{
    public required IUvProvider UvProvider { get; init; }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, in IUvProvider uvProvider) : base(id)
    {
        UvProvider = uvProvider;
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, string textureExtension = "png") : base(id)
    {
        UvProvider = SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{BlockPathPart}.{textureExtension}");
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        return VisualMeshes.FullBlock.Make(UvProvider.Uv);
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
