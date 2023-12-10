using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Registries;
using Sandcube.Worlds.Generation.Meshes;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class SimpleBlock : Block
{
    private Texture? Texture { get; set; }
    protected Rect TextureRect { get; set; }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, in Rect textureRect) : base(id)
    {
        TextureRect = textureRect;
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id, Texture texture) : base(id)
    {
        Texture = texture;
    }

    [SetsRequiredMembers]
    public SimpleBlock(in ModedId id) : this(id, Texture.Load(FileSystem.Mounted, $"textures/{id.ModId}/blocks/{id.Name}.png", true) ?? Texture.Invalid)
    {
    }

    public override void OnRegistered()
    {
        if(Texture is not null)
        {
            TextureRect = SandcubeGame.Instance!.TextureMap.AddTexture(Texture);
            Texture = null;
        }
    }

    public override ISidedMeshPart<ComplexVertex> CreateMesh(BlockState blockState)
    {
        var uv = SandcubeGame.Instance!.TextureMap.GetUv(TextureRect);
        return VisualMeshes.FullBlock.Make(uv);
    }
}
