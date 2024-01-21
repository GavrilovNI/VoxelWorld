using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds.Generation.Meshes;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class SimpleBlock : Block
{
    public required TextureMapPart TexturePart { get; init; }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, in TextureMapPart texturePart) : base(id)
    {
        TexturePart = texturePart;
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, in Rect textureRect) : base(id)
    {
        TexturePart = new(SandcubeGame.Instance!.TextureMap, textureRect);
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, Texture texture) : base(id)
    {
        TexturePart = SandcubeGame.Instance!.TextureMap.AddTexture(texture);
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id) : this(id, Texture.Load(FileSystem.Mounted, $"textures/{id.ModId}/blocks/{id.Name}.png", true) ?? Texture.Invalid)
    {
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        return VisualMeshes.FullBlock.Make(TexturePart.Uv);
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
