using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Mth;
using Sandcube.Registries;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Blocks;

public class SlabBlock : SimpleBlock
{
    protected readonly Vector3[] CenterPositions = new Vector3[4]
    {
        (FullCubeCorners[0] + FullCubeCorners[4]) / 2f,
        (FullCubeCorners[1] + FullCubeCorners[5]) / 2f,
        (FullCubeCorners[2] + FullCubeCorners[6]) / 2f,
        (FullCubeCorners[3] + FullCubeCorners[7]) / 2f,
    };

    public enum SlabType
    {
        Top,
        Bottom,
        Double
    }
    public static BlockProperty<Enum<SlabType>> SlabTypeProperty = new("type", (Enum<SlabType>)SlabType.Bottom);


    public SlabBlock(in ModedId id, in Rect textureRect) : base(id, textureRect)
    {
    }

    public SlabBlock(in ModedId id, Texture texture) : base(id, texture)
    {
    }

    public SlabBlock(in ModedId id) : base(id)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { SlabTypeProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(SlabTypeProperty, (Enum<SlabType>)SlabType.Bottom);

    public override bool IsFullBlock(BlockState blockState) => (SlabType)blockState.GetValue(SlabTypeProperty) == SlabType.Double;

    public override VoxelMesh CreateMesh(BlockState blockState)
    {
        var slabType = (SlabType)blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Double)
            return base.CreateMesh(blockState);

        var uv = SandcubeGame.Instance!.TextureMap.GetUv(TextureRect);
        if(slabType == SlabType.Bottom)
        {
            var sideUv = new Rect(uv.Left, uv.Top + uv.Height / 2f, uv.Width, uv.Height / 2f);

            VoxelMesh voxelMesh = new();
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[3], FullCubeCorners[3], FullCubeCorners[0], CenterPositions[0], Vector3.Backward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Backward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[1], FullCubeCorners[1], FullCubeCorners[2], CenterPositions[2], Vector3.Forward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Forward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[2], FullCubeCorners[2], FullCubeCorners[3], CenterPositions[3], Vector3.Left, VoxelMeshBuilder.TangentUp, sideUv), Direction.Left);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[0], FullCubeCorners[0], FullCubeCorners[1], CenterPositions[1], Vector3.Right, VoxelMeshBuilder.TangentUp, sideUv), Direction.Right);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[2], CenterPositions[3], CenterPositions[0], CenterPositions[1], Vector3.Up, VoxelMeshBuilder.TangentForward, uv));
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[3], FullCubeCorners[2], FullCubeCorners[1], FullCubeCorners[0], Vector3.Down, VoxelMeshBuilder.TangentForward, uv), Direction.Down);
            return voxelMesh;
        }
        else
        {
            var sideUv = new Rect(uv.Left, uv.Top, uv.Width, uv.Height / 2f);
            VoxelMesh voxelMesh = new();
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[7], CenterPositions[3], CenterPositions[0], FullCubeCorners[4], Vector3.Backward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Backward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[5], CenterPositions[1], CenterPositions[2], FullCubeCorners[6], Vector3.Forward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Forward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], CenterPositions[2], CenterPositions[3], FullCubeCorners[7], Vector3.Left, VoxelMeshBuilder.TangentUp, sideUv), Direction.Left);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[4], CenterPositions[0], CenterPositions[1], FullCubeCorners[5], Vector3.Right, VoxelMeshBuilder.TangentUp, sideUv), Direction.Right);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[7], FullCubeCorners[4], FullCubeCorners[5], Vector3.Up, VoxelMeshBuilder.TangentForward, uv), Direction.Up);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[3], CenterPositions[2], CenterPositions[1], CenterPositions[0], Vector3.Down, VoxelMeshBuilder.TangentForward, uv));
            return voxelMesh;
        }
    }
}
