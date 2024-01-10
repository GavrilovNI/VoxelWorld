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

    [Property] protected ModelCollider PhysicsCollider { get; set; } = null!;
    [Property] public ModelCollider InteractionCollider { get; set; } = null!;


    [Property]
    public bool RenderingEnabled
    {
        get => _renderingEnabled;
        set
        {
            _renderingEnabled = value;
            foreach(var renderer in _modelRenderers)
                renderer.Enabled = value;
        }
    }

    [Property]
    public bool PhysicsEnabled
    {
        get => PhysicsCollider?.Enabled ?? false;
        set
        {
            if(PhysicsCollider.IsValid())
                PhysicsCollider.Enabled = value;
        }
    }

    [Property]
    public bool InteractionEnabled
    {
        get => InteractionCollider?.Enabled ?? false;
        set
        {
            if(InteractionCollider.IsValid())
                InteractionCollider.Enabled = value;
        }
    }

    public bool Initialized { get; private set; } = false;

    public IWorldProvider? WorldProvider { get; internal set; }

    public bool ModelsRebuildRequired { get; set; } = false;

    private bool _renderingEnabled = true;
    protected readonly List<ModelRenderer> _modelRenderers = new();

    protected readonly Dictionary<Vector3Int, BlockState> _blockStates = new();


    public BBox Bounds { get; protected set; }

    public Vector3Int BlockOffset => Position * Size;

    public BlockState GetBlockState(Vector3Int position) => _blockStates.GetValueOrDefault(position, BlockState.Air);


    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldProvider? worldProvider)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
        Initialized = true;

        Position = position;
        Size = size;
        WorldProvider = worldProvider;
    }

    protected override void OnEnabled()
    {
        Initialized = true;
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

    protected virtual bool ShouldAddFace(Vector3Int localPosition, BlockState blockState, BlockMeshType meshType, Direction direction)
    {
        var neighborPosition = localPosition + direction;
        var neighborBlockState = GetExternalBlockState(neighborPosition);
        var neighborBlock = neighborBlockState.Block;

        return !neighborBlock.HidesNeighbourFace(neighborBlockState, meshType, direction.GetOpposite());
    }

    protected virtual void AddVisualToMeshBuilder(UnlimitedMesh<ComplexVertex>.Builder opaqueMeshBuilder, UnlimitedMesh<ComplexVertex>.Builder transparentMeshBuilder)
    {
        var meshes = SandcubeGame.Instance!.BlockMeshes;
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            var builder = blockState.Block.Properties.IsTransparent ? transparentMeshBuilder : opaqueMeshBuilder;
            meshes.GetVisual(blockState)!.AddToBuilder(builder, localPosition * MathV.InchesInMeter, visibleFaces);
        }, BlockMeshType.Visual);
    }

    protected virtual void AddPhysicsToMeshBuilder(UnlimitedMesh<Vector3Vertex>.Builder builder)
    {
        var meshes = SandcubeGame.Instance!.BlockMeshes;
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            meshes.GetPhysics(blockState)!.AddToBuilder(builder, localPosition * MathV.InchesInMeter, visibleFaces);
        }, BlockMeshType.Physics);
    }

    protected virtual void AddInteractionToMeshBuilder(UnlimitedMesh<Vector3Vertex>.Builder builder)
    {
        var meshes = SandcubeGame.Instance!.BlockMeshes;
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            meshes.GetInteraction(blockState)!.AddToBuilder(builder, localPosition * MathV.InchesInMeter, visibleFaces);
        }, BlockMeshType.Interaction);
    }

    protected delegate void BuildMeshAction(Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces);
    protected virtual void BuildMesh(BuildMeshAction action, BlockMeshType meshType)
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
                        bool shouldRenderFace = ShouldAddFace(localPosition, blockState, meshType, direction);
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
            var component = GameObject.Components.Create<ModelRenderer>(RenderingEnabled);
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
        UpdateCollider(PhysicsCollider, physicsMeshBuilder);

        PositionOnlyMeshBuilder? interactionMeshBuilder = null;
        if(InteractionCollider.IsValid())
        {
            interactionMeshBuilder = new();
            AddInteractionToMeshBuilder(interactionMeshBuilder);
            UpdateCollider(InteractionCollider, interactionMeshBuilder);
        }

        RecalculateBounds(opaqueMeshBuilder.IsEmpty() ? null : opaqueMeshBuilder,
            transparentMeshBuilder.IsEmpty() ? null : transparentMeshBuilder,
            physicsMeshBuilder.IsEmpty() ? null : physicsMeshBuilder,
            interactionMeshBuilder?.IsEmpty() ?? true ? null : interactionMeshBuilder);
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

    protected virtual void UpdateCollider(ModelCollider collider, PositionOnlyMeshBuilder builder)
    {
        ModelBuilder physicsModelBuilder = new();
        physicsModelBuilder.AddCollisionMesh(builder.CombineVertices().Select(v => v.Position).ToArray(), builder.CombineIndices().ToArray());
        var model = physicsModelBuilder.Create();
        collider.Model = model;
        collider.Enabled = !builder.IsEmpty();
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

        chunk.Initialize(position, size, worldProvider);
        chunk.Enabled = startEnabled;
        return chunk;
    }
}
