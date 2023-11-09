using Sandbox;
using Sandcube.Mth;
using Sandcube.Registries;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds.Blocks;

public class SimpleBlock : Block
{
    private Texture? Texture { get; set; }
    private Rect TextureRect { get; set; }

    public SimpleBlock(ModedId id, Rect textureRect) : base(id)
    {
        TextureRect = textureRect;
    }

    public SimpleBlock(ModedId id, Texture texture) : base(id)
    {
        Texture = texture;
    }

    public SimpleBlock(ModedId id) : this(id, Texture.Load(FileSystem.Mounted, $"textures/{id.ModId}/blocks/{id.Name}.png", true))
    {
    }

    public override void OnRegistered()
    {
        if(Texture is not null)
            TextureRect = SandcubeGame.Instance!.TextureMap.AddTexture(Texture);
    }

    public override VoxelMeshBuilder BuildMesh(Vector3Int voxelPosition, Vector3 voxelSize, HashSet<Direction> exceptSides)
    {
        VoxelMeshBuilder builder = new();
        builder.AddVoxel(voxelPosition, voxelSize, SandcubeGame.Instance!.TextureMap.GetUv(TextureRect), exceptSides);
        return builder;
    }
}
