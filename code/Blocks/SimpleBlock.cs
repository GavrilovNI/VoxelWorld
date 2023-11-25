using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds.Generation;

namespace Sandcube.Blocks;

public class SimpleBlock : Block
{
    //        6_______5
    //       /|      /|
    //     7/_|____4/ |
    //      | |     | | 
    //      | |2____|_|1         z  x
    //      | /     | /          | /
    //      |/______|/        y__|/
    //      3 front 0 
    protected static readonly Vector3[] FullCubeCorners = new Vector3[8]
    {
        Vector3.Zero,
        new Vector3(MathV.InchesInMeter, 0f, 0f),
        new Vector3(MathV.InchesInMeter, MathV.InchesInMeter, 0f),
        new Vector3(0f, MathV.InchesInMeter, 0f),
        new Vector3(0f, 0f, MathV.InchesInMeter),
        new Vector3(MathV.InchesInMeter, 0f, MathV.InchesInMeter),
        new Vector3(MathV.InchesInMeter, MathV.InchesInMeter, MathV.InchesInMeter),
        new Vector3(0f, MathV.InchesInMeter, MathV.InchesInMeter)
    };
    private Texture? Texture { get; set; }
    protected Rect TextureRect { get; set; }

    public SimpleBlock(in ModedId id, in Rect textureRect) : base(id)
    {
        TextureRect = textureRect;
    }

    public SimpleBlock(in ModedId id, Texture texture) : base(id)
    {
        Texture = texture;
    }

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

    public override VoxelMesh CreateMesh(BlockState blockState)
    {
        var uv = SandcubeGame.Instance!.TextureMap.GetUv(TextureRect);

        VoxelMesh voxelMesh = new();
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[7], FullCubeCorners[3], FullCubeCorners[0], FullCubeCorners[4], Vector3.Backward, VoxelMeshBuilder.TangentUp, uv), Direction.Backward);
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[5], FullCubeCorners[1], FullCubeCorners[2], FullCubeCorners[6], Vector3.Forward, VoxelMeshBuilder.TangentUp, uv), Direction.Forward);
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[2], FullCubeCorners[3], FullCubeCorners[7], Vector3.Left, VoxelMeshBuilder.TangentUp, uv), Direction.Left);
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[4], FullCubeCorners[0], FullCubeCorners[1], FullCubeCorners[5], Vector3.Right, VoxelMeshBuilder.TangentUp, uv), Direction.Right);
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[7], FullCubeCorners[4], FullCubeCorners[5], Vector3.Up, VoxelMeshBuilder.TangentForward, uv), Direction.Up);
        voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[3], FullCubeCorners[2], FullCubeCorners[1], FullCubeCorners[0], Vector3.Down, VoxelMeshBuilder.TangentForward, uv), Direction.Down);
        return voxelMesh;
    }
}
