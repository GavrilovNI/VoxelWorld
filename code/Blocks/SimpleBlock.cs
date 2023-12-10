using Sandbox;
using Sandcube.Blocks.Properties;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds.Generation;
using Sandcube.Worlds.Generation.Meshes;
using System.Diagnostics.CodeAnalysis;

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

        SidedMesh<ComplexVertex>.Builder builder = new();
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[7], FullCubeCorners[3], FullCubeCorners[0], FullCubeCorners[4], Vector3.Backward, ComplexMeshBuilder.TangentUp, uv), Direction.Backward);
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[5], FullCubeCorners[1], FullCubeCorners[2], FullCubeCorners[6], Vector3.Forward, ComplexMeshBuilder.TangentUp, uv), Direction.Forward);
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[2], FullCubeCorners[3], FullCubeCorners[7], Vector3.Left, ComplexMeshBuilder.TangentUp, uv), Direction.Left);
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[4], FullCubeCorners[0], FullCubeCorners[1], FullCubeCorners[5], Vector3.Right, ComplexMeshBuilder.TangentUp, uv), Direction.Right);
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[7], FullCubeCorners[4], FullCubeCorners[5], Vector3.Up, ComplexMeshBuilder.TangentForward, uv), Direction.Up);
        builder.Add(new ComplexMeshBuilder().AddQuad(FullCubeCorners[3], FullCubeCorners[2], FullCubeCorners[1], FullCubeCorners[0], Vector3.Down, ComplexMeshBuilder.TangentForward, uv), Direction.Down);
        return builder.Build();
    }
}
