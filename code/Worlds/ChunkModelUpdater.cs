using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Threading;
using Sandcube.Worlds.Generation.Meshes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class ChunkModelUpdater : ThreadHelpComponent
{
    [Property] public Chunk Chunk { get; set; } = null!;

    [Property] public Material OpaqueVoxelsMaterial { get; set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; set; } = null!;

    [Property] protected ModelCollider PhysicsCollider { get; set; } = null!;
    [Property] public ModelCollider InteractionCollider { get; set; } = null!;

    private bool _renderingEnabled = true;
    [Property] public bool RenderingEnabled
    {
        get => _renderingEnabled;
        set
        {
            _renderingEnabled = value;
            foreach(var renderer in ModelRenderers)
                renderer.Enabled = value;
        }
    }
    [Property] public bool PhysicsEnabled
    {
        get => PhysicsCollider?.Enabled ?? false;
        set
        {
            if(PhysicsCollider.IsValid())
                PhysicsCollider.Enabled = value;
        }
    }
    [Property] public bool InteractionEnabled
    {
        get => InteractionCollider?.Enabled ?? false;
        set
        {
            if(InteractionCollider.IsValid())
                InteractionCollider.Enabled = value;
        }
    }

    public BBox Bounds { get; protected set; }


    protected BlockMeshMap BlockMeshMap = null!;
    protected readonly List<ModelRenderer> ModelRenderers = new();


    protected readonly object ModelUpdateLock = new();
    // Should be locked
    protected ModelUpdateStates ModelUpdateState { get; set; } = ModelUpdateStates.Updated;
    protected TaskCompletionSource ModelUpdateTaskSource = new();
    protected CancellationTokenSource? ModelUpdateCancellationTokenSource;


    protected enum ModelUpdateStates
    {
        Updated,
        Required,
        Updating
    }

    protected class MeshBuilders
    {
        public required ComplexMeshBuilder Opaque { get; init; }
        public required ComplexMeshBuilder Translucent { get; init; }
        public required PositionOnlyMeshBuilder Physics { get; init; }
        public required PositionOnlyMeshBuilder Interaction { get; init; }

        [SetsRequiredMembers]
        public MeshBuilders()
        {
            Opaque = new();
            Translucent = new();
            Physics = new();
            Interaction = new();
        }
    }


    // Thread safe
    public virtual Task RequireModelUpdate()
    {
        lock(ModelUpdateLock)
        {
            CancelModelUpdate();
            ModelUpdateState = ModelUpdateStates.Required;
            if(ModelUpdateTaskSource.Task.IsCompleted)
                ModelUpdateTaskSource = new();
            return ModelUpdateTaskSource.Task;
        }
    }

    // Thread safe
    public virtual Task GetModelUpdateTask()
    {
        lock(ModelUpdateLock)
        {
            return ModelUpdateTaskSource.Task;
        }
    }

    public virtual void UpdateTexture(Texture texture)
    {
        foreach(var modelRenderer in ModelRenderers)
            modelRenderer.SceneObject.Attributes.Set("color", texture);
    }

    protected override void OnStart()
    {
        BlockMeshMap = SandcubeGame.Instance!.BlockMeshes;
    }

    protected override void OnDisabled()
    {
        lock(ModelUpdateLock)
            CancelModelUpdate();
    }

    protected override void OnUpdateInner()
    {
        if(!Chunk.Enabled)
            return;

        lock(ModelUpdateLock)
        {
            if(ModelUpdateState == ModelUpdateStates.Required)
                _ = UpdateModels();
        }
    }

    protected override void OnDestroyInner()
    {
        lock(ModelUpdateLock)
        {
            CancelModelUpdate();
            ModelUpdateTaskSource.TrySetCanceled();
        }
    }


    // Thread safe, do not await in non-current thread
    protected virtual void CancelModelUpdate()
    {
        lock(ModelUpdateLock)
        {
            if(ModelUpdateCancellationTokenSource is not null)
            {
                ModelUpdateCancellationTokenSource.Cancel();
                ModelUpdateCancellationTokenSource.Dispose();
                ModelUpdateCancellationTokenSource = null;
            }
            if(ModelUpdateState == ModelUpdateStates.Updating)
                ModelUpdateState = ModelUpdateStates.Required;
        }
    }

    // Thread safe, do not await in non-current thread
    protected virtual CancellationToken RecreateModelUpdateCancellationToken()
    {
        lock(ModelUpdateLock)
        {
            CancelModelUpdate();
            ModelUpdateCancellationTokenSource = new();
            return ModelUpdateCancellationTokenSource.Token;
        }
    }

    protected virtual BlockState? GetExternalBlockState(Vector3Int localPosition)
    {
        if(Chunk.IsInBounds(localPosition))
            return Chunk.GetBlockState(localPosition);

        var world = Chunk.WorldProvider;
        Vector3Int worldPosition = world.GetBlockWorldPosition(Chunk.Position, localPosition);
        var chunkPosition = world.GetChunkPosition(worldPosition);
        if(!world.HasChunk(chunkPosition))
            return null;

        return world.GetBlockState(worldPosition);
    }

    // Thread safe
    protected virtual bool ShouldAddFace(Vector3Int localPosition, BlockState blockState, BlockMeshType meshType, Direction direction)
    {
        var neighborPosition = localPosition + direction;
        var neighborBlockState = GetExternalBlockState(neighborPosition);
        if(neighborBlockState is null)
            return false;

        var neighborBlock = neighborBlockState.Block;
        return !neighborBlock.HidesNeighbourFace(neighborBlockState, meshType, direction.GetOpposite());
    }

    // Call only in game thread
    protected virtual Task UpdateModels(bool force = false)
    {
        ThreadSafe.AssertIsMainThread();

        CancellationToken cancellationToken;
        TaskCompletionSource oldModelUpdateTaskSource;

        lock(ModelUpdateLock)
        {
            if(force || ModelUpdateState == ModelUpdateStates.Required)
                cancellationToken = RecreateModelUpdateCancellationToken();
            else
                throw new InvalidOperationException($"Can't update models. {nameof(ModelUpdateState)} was not {ModelUpdateStates.Required} on chunk {Chunk.Position}, was {ModelUpdateState}");

            ModelUpdateState = ModelUpdateStates.Updating;
            oldModelUpdateTaskSource = ModelUpdateTaskSource;
        }

        MeshBuilders builders = new();

        _ = Task.RunInThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await GenerateMeshes(builders, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await RunInGameThread(async ct =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ApplyMeshes(builders, cancellationToken);

                lock(ModelUpdateLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if(ModelUpdateState != ModelUpdateStates.Updating)
                        throw new InvalidOperationException($"Can't update models. {nameof(ModelUpdateState)} was not {ModelUpdateStates.Updating} on chunk {Chunk.Position} when update finished, was {ModelUpdateState}");

                    ModelUpdateState = ModelUpdateStates.Updated;
                    if(!oldModelUpdateTaskSource.TrySetResult())
                    {
                        ModelUpdateState = ModelUpdateStates.Required;
                        throw new InvalidOperationException($"Can't finish model update task on chunk {Chunk.Position}");
                    }
                }
            }, cancellationToken);
        });

        return oldModelUpdateTaskSource.Task;
    }


    // Call only in game thread
    protected virtual void DestroyModelRenderers()
    {
        ThreadSafe.AssertIsMainThread();

        foreach(var modelComponent in ModelRenderers)
            modelComponent.Destroy();
        ModelRenderers.Clear();
    }

    // Call only in game thread
    protected virtual List<ModelRenderer> AddModelRenderers(int count)
    {
        ThreadSafe.AssertIsMainThread();

        List<ModelRenderer> result = new(count);
        for(int i = 0; i < count; ++i)
        {
            var component = GameObject.Components.Create<ModelRenderer>(RenderingEnabled);
            result.Add(component);
            ModelRenderers.Add(component);
        }
        return result;
    }

    // call only in main thread
    protected virtual void UpdateModelRenderers(ComplexMeshBuilder opaqueBuilder, ComplexMeshBuilder translucentBuilder, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        bool isEmpty = opaqueBuilder.PartsCount == 0 && translucentBuilder.PartsCount == 0;
        if(isEmpty)
            return;

        var opaqueModels = CreateRenderModels(opaqueBuilder, OpaqueVoxelsMaterial);
        var translucentModels = CreateRenderModels(translucentBuilder, TranslucentVoxelsMaterial);

        DestroyModelRenderers();
        var opaqueRenderers = AddModelRenderers(opaqueModels.Length);
        var translucentRenderers = AddModelRenderers(translucentModels.Length);

        var blocksTexture = SandcubeGame.Instance!.BlocksTextureMap.Texture;
        for(int i = 0; i < opaqueModels.Length; ++i)
        {
            opaqueRenderers[i].Model = opaqueModels[i];
            opaqueRenderers[i].SceneObject.Attributes.Set("color", blocksTexture);
        }

        for(int i = 0; i < translucentModels.Length; ++i)
        {
            translucentRenderers[i].Model = translucentModels[i];
            translucentRenderers[i].SceneObject.Attributes.Set("color", blocksTexture);
        }

        Model[] CreateRenderModels(ComplexMeshBuilder builder, Material material)
        {
            ThreadSafe.AssertIsMainThread();

            var partsCount = builder.PartsCount;
            cancellationToken.ThrowIfCancellationRequested();

            Model[] models = new Model[partsCount];

            for(int i = 0; i < partsCount; ++i)
            {
                ModelBuilder modelBuilder = new();
                Mesh mesh = new(material);
                builder.CreateBuffersFor(mesh, i);
                modelBuilder.AddMesh(mesh);
                models[i] = modelBuilder.Create();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return models;
        }
    }

    // Thread safe
    protected virtual Task UpdateCollider(ModelCollider collider, PositionOnlyMeshBuilder builder, CancellationToken cancellationToken)
    {
        return Task.RunInThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ModelBuilder physicsModelBuilder = new();
            builder.AddAsCollisionMesh(physicsModelBuilder);

            await RunInGameThread(ct =>
            {
                var model = physicsModelBuilder.Create(); // should be called in game thread as it's not thread safe
                cancellationToken.ThrowIfCancellationRequested();
                collider.Model = model;
                collider.Enabled = !builder.IsEmpty();
            }, cancellationToken);

        });
    }



    // Thread safe
    protected virtual Task GenerateMeshes(MeshBuilders builders, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<Task> tasks = new()
        {
            Task.RunInThreadAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                BuildVisualMesh(builders.Opaque, builders.Translucent);
            }),
            Task.RunInThreadAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                BuildPhysicsMesh(builders.Physics);
            }),
            Task.RunInThreadAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                BuildInteractionMesh(builders.Interaction);
            })
        };

        cancellationToken.ThrowIfCancellationRequested();
        return Task.WhenAll(tasks);
    }

    // Call only in game thread
    protected virtual async Task ApplyMeshes(MeshBuilders builders, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        cancellationToken.ThrowIfCancellationRequested();
        var physicsTask = UpdateCollider(PhysicsCollider, builders.Physics, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        var interactionTask = UpdateCollider(InteractionCollider, builders.Interaction, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        UpdateModelRenderers(builders.Opaque, builders.Translucent, cancellationToken);

        await Task.WhenAll(physicsTask, interactionTask);

        cancellationToken.ThrowIfCancellationRequested();

        RecalculateBounds(builders.Opaque.Bounds,
            builders.Translucent.Bounds,
            builders.Physics.Bounds,
            builders.Interaction.Bounds);

        cancellationToken.ThrowIfCancellationRequested();
    }

    protected virtual void RecalculateBounds(params BBox[] bounds)
    {
        ThreadSafe.AssertIsMainThread();

        BBox? result = null;
        foreach(var currentBounds in bounds)
        {
            if(!currentBounds.Size.AlmostEqual(0))
                result = result.AddOrCreate(currentBounds);
        }
        result ??= new BBox();

        Bounds = result.Value.Translate(Transform.Position);
    }


    #region Build Mesh Methods
    // Thread safe
    protected virtual void BuildVisualMesh(UnlimitedMesh<ComplexVertex>.Builder opaqueMeshBuilder, UnlimitedMesh<ComplexVertex>.Builder transparentMeshBuilder)
    {
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            var builder = blockState.Block.Properties.IsTransparent ? transparentMeshBuilder : opaqueMeshBuilder;
            BlockMeshMap.GetVisual(blockState)!.AddToBuilder(builder, localPosition * MathV.UnitsInMeter, visibleFaces);
        }, BlockMeshType.Visual);
    }

    // Thread safe
    protected virtual void BuildPhysicsMesh(UnlimitedMesh<Vector3Vertex>.Builder builder)
    {
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            BlockMeshMap.GetPhysics(blockState)!.AddToBuilder(builder, localPosition * MathV.UnitsInMeter, visibleFaces);
        }, BlockMeshType.Physics);
    }

    // Thread safe
    protected virtual void BuildInteractionMesh(UnlimitedMesh<Vector3Vertex>.Builder builder)
    {
        BuildMesh((Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            BlockMeshMap.GetInteraction(blockState)!.AddToBuilder(builder, localPosition * MathV.UnitsInMeter, visibleFaces);
        }, BlockMeshType.Interaction);
    }

    protected delegate void BuildMeshAction(Vector3Int localPosition, BlockState blockState, HashSet<Direction> visibleFaces);
    // Thread safe
    protected virtual void BuildMesh(BuildMeshAction action, BlockMeshType meshType)
    {
        HashSet<Direction> visibleFaces = new();
        for(int x = 0; x < Chunk.Size.x; ++x)
        {
            for(int y = 0; y < Chunk.Size.y; ++y)
            {
                for(int z = 0; z < Chunk.Size.z; ++z)
                {
                    Vector3Int localPosition = new(x, y, z);
                    var blockState = Chunk.GetBlockState(localPosition);
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
    #endregion

}
