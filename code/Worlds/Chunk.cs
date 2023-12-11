using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Worlds.Generation.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Worlds;

public class Chunk : Component, IBlockStateAccessor
{
    [Property] public Vector3Int Position { get; internal set; }
    [Property] public Vector3Int Size { get; internal set; } = 16;

    [Property] public Material OpaqueVoxelsMaterial { get; set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; set; } = null!;

    public IWorldProvider? WorldProvider { get; internal set; }

    public bool ModelsRebuildRequired { get; set; } = false;

    protected readonly List<ModelRenderer> _modelRenderers = new();
    protected ModelCollider _modelCollider = null!;

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
        ModelsRebuildRequired = true;
    }


    protected override void OnStart()
    {
        _modelCollider = GameObject.Components.Create<ModelCollider>();
    }

    protected override void OnUpdate()
    {
        if(ModelsRebuildRequired)
            UpdateModel();
    }

    public virtual void Clear()
    {
        _blockStates.Clear();
        ModelsRebuildRequired = true;
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

    protected virtual void AddVisualToMeshBuilder(ComplexMeshBuilder opaqueMeshBuilder, ComplexMeshBuilder transparentMeshBuilder)
    {
        var meshes = SandcubeGame.Instance!.BlockMeshes;
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            var builder = blockState.Block.Properties.IsTransparent ? transparentMeshBuilder : opaqueMeshBuilder;
            meshes.AddVisualToMeshBuilder(blockState, builder, localPosition * MathV.InchesInMeter, visibleFaces);
        });
    }

    protected virtual void AddPhysicsToMeshBuilder(PositionOnlyMeshBuilder builder)
    {
        var meshes = SandcubeGame.Instance!.BlockMeshes;
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            meshes.AddPhysicsToMeshBuilder(blockState, builder, localPosition * MathV.InchesInMeter, visibleFaces);
        });
    }

    protected delegate void BuildMeshAction(Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces);
    protected virtual void BuildMesh(BuildMeshAction action)
    {
        HashSet<Direction> visibleFaces = new();
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
                    action(localPosition, blockState, visibleFaces);
                }
            }
        }
    }

    protected virtual void DestroyModelRenderers()
    {
        foreach(var modelComponent in _modelRenderers)
            modelComponent.Destroy();
        _modelRenderers.Clear();
    }

    protected virtual List<ModelRenderer> AddModelRenders(int count)
    {
        List<ModelRenderer> result = new(count);
        for(int i = 0; i < count; ++i)
        {
            var component = GameObject.Components.Create<ModelRenderer>();
            result.Add(component);
            _modelRenderers.Add(component);
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

        ModelsRebuildRequired = true;
    }

    protected virtual void UpdateModel()
    {
        ModelsRebuildRequired = false;

        ComplexMeshBuilder opaqueMeshBuilder = new();
        ComplexMeshBuilder transparentMeshBuilder = new();
        AddVisualToMeshBuilder(opaqueMeshBuilder, transparentMeshBuilder);
        DestroyModelRenderers();
        AddRenderers(opaqueMeshBuilder, OpaqueVoxelsMaterial);
        AddRenderers(transparentMeshBuilder, TranslucentVoxelsMaterial);

        PositionOnlyMeshBuilder physicsMeshBuilder = new();
        AddPhysicsToMeshBuilder(physicsMeshBuilder);
        UpdateCollider(physicsMeshBuilder);

        RecalculateBounds(opaqueMeshBuilder.IsEmpty() ? null : opaqueMeshBuilder,
            transparentMeshBuilder.IsEmpty() ? null : transparentMeshBuilder,
            physicsMeshBuilder.IsEmpty() ? null : physicsMeshBuilder);
    }

    protected virtual void RecalculateBounds(params IBounded?[] bounders)
    {
        BBox? bounds = null;
        foreach(var currentBounder in bounders)
        {
            if(currentBounder is null)
                continue;
            if(bounds.HasValue)
                bounds = bounds.Value.AddBBox(currentBounder.Bounds);
            else
                bounds = currentBounder.Bounds;
        }

        if(!bounds.HasValue)
            bounds = new BBox();

        Bounds = bounds.Value.Translate(Transform.Position);
    }

    protected virtual void AddRenderers(ComplexMeshBuilder builder, Material material)
    {
        var buffers = builder.ToVertexBuffers();
        bool isEmpty = buffers.Count == 0;
        if(isEmpty)
            return;

        var modelComponents = AddModelRenders(buffers.Count);

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

    protected virtual void UpdateCollider(PositionOnlyMeshBuilder builder)
    {
        ModelBuilder physicsModelBuilder = new();
        physicsModelBuilder.AddCollisionMesh(builder.CombineVertices().Select(v => v.Position).ToArray(), builder.CombineIndices().ToArray());
        var model = physicsModelBuilder.Create();
        _modelCollider.Model = model;
        _modelCollider.Enabled = !builder.IsEmpty();
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
