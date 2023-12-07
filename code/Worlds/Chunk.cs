using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;
using System.Drawing;

namespace Sandcube.Worlds;

public class Chunk : Component, IBlockStateAccessor
{
    [Property] public Vector3Int Position { get; internal set; }
    [Property] public Vector3Int Size { get; internal set; } = 16;

    [Property] public Material OpaqueVoxelsMaterial { get; set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; set; } = null!;

    public IWorldProvider? WorldProvider { get; internal set; }

    public bool MeshRebuildRequired { get; set; } = false;

    protected readonly List<ModelRenderer> _modelComponents = new();
    protected ModelCollider _opaqueModelCollider = null!;
    protected ModelCollider _transparentModelCollider = null!;

    protected readonly Dictionary<Vector3Int, BlockState> _blockStates = new();


    public BBox Bounds { get; protected set; }

    public Vector3Int BlockOffset => Position * Size;

    public BlockState GetBlockState(Vector3Int position) => _blockStates.GetValueOrDefault(position, BlockState.Air);

    public Chunk()
    {

    }

    public void SetBlockState(Vector3Int position, BlockState blockState)
    {
        if(!IsInBounds(position))
            throw new ArgumentOutOfRangeException(nameof(position), position, "block position is out of chunk bounds");

        if(GetBlockState(position) == blockState)
            return;

        _blockStates[position] = blockState;
        MeshRebuildRequired = true;
    }


    protected override void OnStart()
    {
        _opaqueModelCollider = GameObject.Components.Create<ModelCollider>();
        _transparentModelCollider = GameObject.Components.Create<ModelCollider>();
    }

    protected override void OnUpdate()
    {
        if(MeshRebuildRequired)
            UpdateModel();
    }

    public virtual void Clear()
    {
        _blockStates.Clear();
        MeshRebuildRequired = true;
        Bounds = default;
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

    protected virtual List<ModelRenderer> AddModelComponents(int count)
    {
        List<ModelRenderer> result = new(count);
        for(int i = 0; i < count; ++i)
        {
            var component = GameObject.Components.Create<ModelRenderer>();
            result.Add(component);
            _modelComponents.Add(component);
        }
        return result;
    }

    public virtual void OnNeighbouringChunkEdgeUpdated(Direction directionToNeighbouringChunk,
        Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        if(WorldProvider is not null)
        {
            var sidedBlockPosition = updatedBlockPosition - directionToNeighbouringChunk;
            var sidedLocalBlockPosition = WorldProvider.GetBlockPositionInChunk(sidedBlockPosition);
            var sidedBlockState = GetBlockState(sidedLocalBlockPosition);
            if(sidedBlockState.IsAir())
                return;
        }

        MeshRebuildRequired = true;
    }

    protected virtual void UpdateModel()
    {
        MeshRebuildRequired = false;
        DestroyModelComponents();

        VoxelMeshBuilder opaqueMeshBuilder = new();
        VoxelMeshBuilder transparentMeshBuilder = new();
        AddVoxelsToMeshBuilder(opaqueMeshBuilder, transparentMeshBuilder);

        Bounds = opaqueMeshBuilder.Bounds;

        AddModel(opaqueMeshBuilder, _opaqueModelCollider, OpaqueVoxelsMaterial);
        AddModel(transparentMeshBuilder, _transparentModelCollider, TranslucentVoxelsMaterial);

        RecalculateBounds(opaqueMeshBuilder, transparentMeshBuilder);
    }

    protected virtual void RecalculateBounds(params VoxelMeshBuilder[] meshBuilders)
    {
        BBox? bounds = null;
        foreach(var builder in meshBuilders)
        {
            if(builder.IsEmpty())
                continue;
            if(bounds.HasValue)
                bounds = bounds.Value.AddBBox(builder.Bounds);
            else
                bounds = builder.Bounds;
        }

        if(!bounds.HasValue)
            bounds = new BBox();

        Bounds = bounds.Value.Translate(Transform.Position);
    }

    protected virtual void AddModel(VoxelMeshBuilder builder, ModelCollider collider, Material material)
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
            Mesh mesh = new(material);
            mesh.CreateBuffers(buffers[i]);
            modelBuilder.AddMesh(mesh);
            modelComponents[i].Model = modelBuilder.Create();
            modelComponents[i].SceneObject.Attributes.Set("color", SandcubeGame.Instance!.TextureMap.Texture);
        }
    }

    protected override void DrawGizmos()
    {
        var bounds = Bounds.Translate(-Transform.Position).Expanded(1);
        Gizmo.Hitbox.BBox(bounds);
    }
}

public static class ComponentListChunkExtensions
{
    public static T Create<T>(this ComponentList components, Vector3Int position, Vector3Int size, IWorldProvider? worldProvider, bool startEnabled = true) where T : Chunk, new()
    {
        var chunk = components.Create<T>(false);

        chunk.Position = position;
        chunk.Size = size;
        chunk.WorldProvider = worldProvider;
        chunk.Enabled = startEnabled;

        return chunk;
    }
}
