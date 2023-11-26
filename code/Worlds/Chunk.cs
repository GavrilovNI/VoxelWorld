using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds;

public class Chunk : BaseComponent, IBlockStateAccessor
{
    [Property] public Vector3Int Position { get; init; }
    [Property] public Vector3Int Size { get; init; } = 16;

    [Property] public Material VoxelsMaterial { get; set; } = null!;

    protected readonly IWorldProvider? WorldProvider;

    protected bool _meshRebuildRequired = false;

    protected readonly List<ModelComponent> _modelComponents = new();
    protected ModelCollider _opaqueModelCollider = null!;
    protected ModelCollider _transparentModelCollider = null!;

    protected readonly Dictionary<Vector3Int, BlockState> _blockStates = new();

    public Vector3Int BlockOffset => Position * Size;

    public Chunk()
    {
    }

    public Chunk(Vector3Int position, Vector3Int size, IWorldProvider? worldProvider = null)
    {
        Position = position;
        Size = size;
        WorldProvider = worldProvider;
    }

    public BlockState GetBlockState(Vector3Int position) => _blockStates.GetValueOrDefault(position, BlockState.Air);

    public void SetBlockState(Vector3Int position, BlockState blockState)
    {
        if(!IsInBounds(position))
            throw new ArgumentOutOfRangeException(nameof(position), position, "block position is out of chunk bounds");

        if(GetBlockState(position) == blockState)
            return;

        _blockStates[position] = blockState;
        _meshRebuildRequired = true;
    }


    public override void OnStart()
    {
        _opaqueModelCollider = GameObject.AddComponent<ModelCollider>();
        _transparentModelCollider = GameObject.AddComponent<ModelCollider>();
    }

    public override void Update()
    {
        if(_meshRebuildRequired)
            UpdateModel();
    }

    public virtual void Clear()
    {
        _blockStates.Clear();
        _meshRebuildRequired = true;
    }

    protected virtual bool IsInBounds(Vector3Int position) => !position.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a));

    protected virtual BlockState GetExternalBlockState(Vector3Int localPosition)
    {
        if(IsInBounds(localPosition))
            return GetBlockState(localPosition);

        if(WorldProvider is null)
            return BlockState.Air;

        Vector3Int globalPosition = WorldProvider.GetBlockWorldPosition(Position, localPosition);
        return WorldProvider.GetBlockState(globalPosition);
    }

    protected virtual bool ShouldRenderFace(Vector3Int localPosition, BlockState blockState, Direction direction)
    {
        var neighborPosition = localPosition + direction;
        var neighborBlockState = GetExternalBlockState(neighborPosition);
        var neighborBlock = neighborBlockState.Block;

        return neighborBlockState.IsAir() ||
            neighborBlock.Properties.IsTransparent ||
            !neighborBlock.IsFullBlock(neighborBlockState);
    }

    protected virtual void AddVoxelsToMeshBuilder(VoxelMeshBuilder opaqueMeshBuilder, VoxelMeshBuilder transparentMeshBuilder)
    {
        HashSet<Direction> visibleFaces = new();
        var meshes = SandcubeGame.Instance!.BlockMeshes;

        for(int x = 0; x < Size.x; ++x)
        {
            for(int y = 0; y < Size.y; ++y)
            {
                for(int z = 0; z < Size.z; ++z)
                {
                    Vector3Int localPosition = new(x, y, z);
                    var blockState = GetBlockState(localPosition);
                    if(blockState.IsAir())
                        continue;

                    foreach(var direction in Direction.All)
                    {
                        bool shouldRenderFace = ShouldRenderFace(localPosition, blockState, direction);
                        if(shouldRenderFace)
                            visibleFaces.Add(direction);
                        else
                            visibleFaces.Remove(direction);
                    }
                    var builder = blockState.Block.Properties.IsTransparent ? transparentMeshBuilder : opaqueMeshBuilder;
                    meshes.BuildAt(builder, blockState, localPosition * MathV.InchesInMeter, visibleFaces);
                }
            }
        }
    }

    protected virtual void DestroyModelComponents()
    {
        foreach(var modelComponent in _modelComponents)
            modelComponent.Destroy();
        _modelComponents.Clear();
    }

    protected virtual List<ModelComponent> AddModelComponents(int count)
    {
        List<ModelComponent> result = new(count);
        for(int i = 0; i < count; ++i)
        {
            var component = GameObject.AddComponent<ModelComponent>();
            result.Add(component);
            _modelComponents.Add(component);
        }
        return result;
    }

    protected virtual void UpdateModel()
    {
        _meshRebuildRequired = false;
        DestroyModelComponents();

        VoxelMeshBuilder opaqueMeshBuilder = new();
        VoxelMeshBuilder transparentMeshBuilder = new();
        AddVoxelsToMeshBuilder(opaqueMeshBuilder, transparentMeshBuilder);

        AddModel(opaqueMeshBuilder, _opaqueModelCollider);
        AddModel(transparentMeshBuilder, _transparentModelCollider);
    }

    protected virtual void AddModel(VoxelMeshBuilder builder, ModelCollider collider)
    {
        var buffers = builder.ToVertexBuffers();
        bool isEmpty = buffers.Count == 0;
        collider.Enabled = !isEmpty;
        if(isEmpty)
            return;

        var modelComponents = AddModelComponents(buffers.Count);

        ModelBuilder physicsModelBuilder = new();
        builder.AddAsCollisionMesh(physicsModelBuilder);
        var model = physicsModelBuilder.Create();
        collider.Model = model;

        for(int i = 0; i < buffers.Count; ++i)
        {
            ModelBuilder modelBuilder = new();
            Mesh mesh = new(VoxelsMaterial);
            mesh.CreateBuffers(buffers[i]);
            modelBuilder.AddMesh(mesh);
            modelComponents[i].Model = modelBuilder.Create();
            modelComponents[i].SceneObject.Attributes.Set("color", SandcubeGame.Instance!.TextureMap.Texture);
        }
    }
}
