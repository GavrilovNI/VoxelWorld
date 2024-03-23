using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VoxelWorld.Rendering;

namespace VoxelWorld.Worlds;

public class ChunkModelUpdater : Component
{
    [Property] public Chunk Chunk { get; set; } = null!;

    [Property] public Material OpaqueVoxelsMaterial { get; set; } = null!;
    [Property] public Material TranslucentVoxelsMaterial { get; set; } = null!;

    [Property] protected ModelCollider PhysicsCollider { get; set; } = null!;
    [Property] public ModelCollider InteractionCollider { get; set; } = null!;

    [Property] protected UnlimitedModelRenderer OpaqueModelRenderer { get; set; } = null!;
    [Property] protected UnlimitedModelRenderer TranslucentModelRenderer { get; set; } = null!;

    [Property] public bool RenderingEnabled
    {
        get => (OpaqueModelRenderer.IsValid() && OpaqueModelRenderer.Enabled) &&
            (TranslucentModelRenderer.IsValid() && TranslucentModelRenderer.Enabled);
        set 
        {
            if(OpaqueModelRenderer.IsValid())
                OpaqueModelRenderer.Enabled = value;
            if(TranslucentModelRenderer.IsValid())
                TranslucentModelRenderer.Enabled = value;
        }
    }
    [Property] public bool PhysicsEnabled
    {
        get => PhysicsCollider.IsValid() && PhysicsCollider.Enabled;
        set
        {
            if(PhysicsCollider.IsValid())
                PhysicsCollider.Enabled = value;
        }
    }
    [Property] public bool InteractionEnabled
    {
        get => InteractionCollider.IsValid() && InteractionCollider.Enabled;
        set
        {
            if(InteractionCollider.IsValid())
                InteractionCollider.Enabled = value;
        }
    }

    public BBox ModelBounds { get; protected set; }


    protected BlockMeshMap BlockMeshMap = null!;


    protected readonly object ModelUpdateLock = new();
    // Should be locked
    protected ModelUpdateStates ModelUpdateState { get; set; } = ModelUpdateStates.Updated;
    protected TaskCompletionSource ModelUpdateTaskSource;
    protected CancellationTokenSource? ModelUpdateCancellationTokenSource;

    public ChunkModelUpdater()
    {
        ModelUpdateTaskSource = new();
        ModelUpdateTaskSource.SetResult();
    }

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
        public required ModelBuilder Physics { get; init; }
        public required ModelBuilder Interaction { get; init; }

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

    protected override void OnStart()
    {
        var game = GameController.Instance!;
        BlockMeshMap = game.BlockMeshes;
        OpaqueVoxelsMaterial = game.OpaqueVoxelsMaterial;
        TranslucentVoxelsMaterial = game.TranslucentVoxelsMaterial;
    }

    protected override void OnDisabled()
    {
        lock(ModelUpdateLock)
            CancelModelUpdate();
    }

    protected override void OnUpdate()
    {
        if(!Chunk.Enabled)
            return;

        lock(ModelUpdateLock)
        {
            if(ModelUpdateState == ModelUpdateStates.Required)
                _ = UpdateModels();
        }
    }

    protected override void OnDestroy()
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

    protected virtual BlockState? GetExternalBlockState(Vector3IntB localPosition)
    {
        if(Chunk.IsInBounds(localPosition))
            return Chunk.GetBlockState(localPosition);

        var world = Chunk.World;
        var worldPosition = world.GetBlockWorldPosition(Chunk.Position, localPosition);

        if(!world.Limits.Contains(worldPosition))
            return BlockState.Air;

        var chunkPosition = world.GetChunkPosition(worldPosition);
        if(!world.HasChunk(chunkPosition))
            return null;

        return world.GetBlockState(worldPosition);
    }

    // Thread safe
    protected virtual bool ShouldAddFace(Vector3IntB localPosition, BlockState blockState, BlockMeshType meshType, Direction direction)
    {
        var neighborPosition = localPosition + direction;
        var neighborBlockState = GetExternalBlockState(neighborPosition);
        if(neighborBlockState is null)
            return false;

        var neighborBlock = neighborBlockState.Block;

        if(neighborBlock.HidesNeighbourFace(neighborBlockState, meshType, direction.GetOpposite()))
            return false;

        return blockState.Block.ShouldAddFace(blockState, meshType, direction, neighborBlockState);
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

            await Task.RunInMainThreadAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyMeshes(builders, cancellationToken);

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
            });
        });

        return oldModelUpdateTaskSource.Task;
    }

    // call only in main thread
    protected virtual void UpdateModelRenderers(ComplexMeshBuilder opaqueBuilder, ComplexMeshBuilder translucentBuilder, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        bool isEmpty = opaqueBuilder.PartsCount == 0 && translucentBuilder.PartsCount == 0;
        if(isEmpty)
        {
            OpaqueModelRenderer.Clear();
            TranslucentModelRenderer.Clear();
            return;
        }

        var blocksTexture = GameController.Instance!.BlocksTextureMap.Texture;

        OpaqueModelRenderer.SetModels(opaqueBuilder.Build(), OpaqueVoxelsMaterial);
        OpaqueModelRenderer.SceneObjectAttrubutes.Set("color", blocksTexture);

        TranslucentModelRenderer.SetModels(translucentBuilder.Build(), TranslucentVoxelsMaterial);
        TranslucentModelRenderer.SceneObjectAttrubutes.Set("color", blocksTexture);
    }

    // call only in main thread
    protected virtual void UpdateCollider(ModelCollider collider, ModelBuilder builder, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        var model = builder.Create();
        cancellationToken.ThrowIfCancellationRequested();
        collider.Model = model;
        collider.Enabled = !model.PhysicsBounds.Size.AlmostEqual(Vector3.Zero);
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
    protected virtual void ApplyMeshes(MeshBuilders builders, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        cancellationToken.ThrowIfCancellationRequested();
        UpdateCollider(PhysicsCollider, builders.Physics, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        UpdateCollider(InteractionCollider, builders.Interaction, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        UpdateModelRenderers(builders.Opaque, builders.Translucent, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        RecalculateBounds(builders.Opaque.Bounds,
            builders.Translucent.Bounds,
            PhysicsCollider.Model.PhysicsBounds,
            InteractionCollider.Model.PhysicsBounds);

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
        ModelBounds = result ?? new BBox();
    }


    #region Build Mesh Methods
    // Thread safe
    protected virtual void BuildVisualMesh(UnlimitedMesh<ComplexVertex>.Builder opaqueMeshBuilder, UnlimitedMesh<ComplexVertex>.Builder transparentMeshBuilder)
    {
        BuildMesh((Vector3IntB localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            var builder = blockState.Block.Properties.IsTransparent ? transparentMeshBuilder : opaqueMeshBuilder;
            BlockMeshMap.GetVisual(blockState)!.AddToBuilder(builder, visibleFaces, localPosition * MathV.UnitsInMeter);
        }, BlockMeshType.Visual);
    }

    // Thread safe
    protected virtual void BuildPhysicsMesh(ModelBuilder builder)
    {
        BuildMesh((Vector3IntB localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            BlockMeshMap.GetPhysics(blockState)!.AddAsCollisionMesh(builder, visibleFaces, localPosition * MathV.UnitsInMeter);
        }, BlockMeshType.Physics);
    }

    // Thread safe
    protected virtual void BuildInteractionMesh(ModelBuilder builder)
    {
        BuildMesh((Vector3IntB localPosition, BlockState blockState, HashSet<Direction> visibleFaces) =>
        {
            BlockMeshMap.GetInteraction(blockState)!.AddAsCollisionMesh(builder, visibleFaces, localPosition * MathV.UnitsInMeter);
        }, BlockMeshType.Interaction);
    }

    protected delegate void BuildMeshAction(Vector3IntB localPosition, BlockState blockState, HashSet<Direction> visibleFaces);
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
                    Vector3IntB localPosition = new(x, y, z);
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
