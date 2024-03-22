using Sandbox;
using VoxelWorld.Blocks;
using VoxelWorld.Blocks.States;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Items;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Sandbox.Physics;
using VoxelWorld.Worlds;

namespace VoxelWorld.Entities;

public class PhysicsBlockEntity : Entity, Component.ICollisionListener
{
    [Property] protected Rigidbody Rigidbody { get; set; } = null!;
    [Property] protected ModelCollider Collider { get; set; } = null!;

    protected List<ModelRenderer> Renderers { get; set; } = new();

    public BBox ModelBounds { get; protected set; }
    public BlockState BlockState { get; private set; } = null!;
    public ConvertionStatus ConvertionStatus { get; protected set; } = ConvertionStatus.None;
    protected IWorldAccessor? ConvertingWorld { get; set; } = null!;
    protected object ConvertionStatusLocker { get; } = new();

    protected override void OnAwakeChild()
    {
        if(BlockState is null)
            SetBlockState(BlockState.Air);
    }


    public virtual void SetBlockState(BlockState blockState)
    {
        ThreadSafe.AssertIsMainThread();

        BlockState = blockState;
        if(BlockState.IsAir())
        {
            Collider.Enabled = false;
            foreach(var renderer in Renderers)
                renderer.Destroy();
            Renderers.Clear();
            Rigidbody.Enabled = false;
            RecalculateBounds();
            return;
        }

        var game = GameController.Instance!;
        var blockMeshes = game.BlockMeshes;

        var physicsMesh = blockMeshes.GetPhysics(blockState)!;
        if(physicsMesh.Bounds.Size.AlmostEqual(0))
            physicsMesh = PhysicsMeshes.FullBlock;

        physicsMesh = new SidedMesh<Vector3Vertex>.Builder().Add(physicsMesh).Scale(physicsMesh.Bounds.Center, 0.99f);
        ModelBuilder physicsModelBuilder = new();
        physicsMesh.AddAsCollisionHull(physicsModelBuilder, Vector3.Zero, Rotation.Identity);
        Collider.Model = physicsModelBuilder.Create();

        var isTransparent = blockState.Block.Properties.IsTransparent;
        var material = isTransparent ? game.TranslucentVoxelsMaterial : game.OpaqueVoxelsMaterial;

        var visualMesh = blockMeshes.GetVisual(blockState)!;
        ComplexMeshBuilder visualMeshBuilder = new ComplexMeshBuilder().Add(visualMesh);
        for(int i = 0; i < visualMeshBuilder.PartsCount; ++i)
        {
            ModelBuilder modelBuilder = new();
            Mesh mesh = new(material);
            visualMeshBuilder.CreateBuffersFor(mesh, i);
            modelBuilder.AddMesh(mesh);

            var renderer = Components.Create<ModelRenderer>();
            Renderers.Add(renderer);
            renderer.Model = modelBuilder.Create();
        }

        Rigidbody.Enabled = true;
        RecalculateBounds(physicsMesh.Bounds, visualMeshBuilder.Bounds);
    }

    protected override void DrawGizmosChild()
    {
        Gizmo.Hitbox.BBox(ModelBounds);
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

    public virtual void OnCollisionStart(Collision other)
    {
        lock(ConvertionStatusLocker)
        {
            if(ConvertionStatus != ConvertionStatus.None)
                return;
        }

        if(!Worlds.World.TryFindInObject(other.Other.GameObject, out var world))
            return;

        if(Direction.ClosestTo(other.Contact.Normal) == Direction.Down)
            RequestConvertingToBlockOrDrop(world);
    }

    public virtual void OnCollisionUpdate(Collision other) { }
    public virtual void OnCollisionStop(CollisionStop other) { }

    public virtual void RequestConvertingToBlockOrDrop() => RequestConvertingToBlockOrDrop(World!);

    public virtual void RequestConvertingToBlockOrDrop(IWorldAccessor world)
    {
        lock(ConvertionStatusLocker)
        {
            if(ConvertionStatus != ConvertionStatus.None)
                return;

            ConvertionStatus = ConvertionStatus.ConvertionRequested;
            ConvertingWorld = world;
            Rigidbody.Enabled = false;
        }
    }

    protected virtual async Task ConvertToBlockOrDrop()
    {
        lock(ConvertionStatusLocker)
        {
            if(ConvertionStatus == ConvertionStatus.Converting || ConvertionStatus == ConvertionStatus.Converted)
                throw new InvalidOperationException($"{nameof(PhysicsBlockEntity)} was already converting or converted");
            ConvertionStatus = ConvertionStatus.Converting;
        }

        if(BlockState.IsAir())
        {
            Destroy();
            lock(ConvertionStatusLocker)
            {
                ConvertionStatus = ConvertionStatus.Converted;
            }
            return;
        }

        Vector3 testPosition = Transform.Position + ModelBounds.Center.WithZ(ModelBounds.Mins.z);
        Vector3IntB blockPosition = Vector3IntB.Zero;
        BlockState currentState = BlockState.Air;
        var canPlace = ConvertingWorld is not null;

        if(canPlace)
        {
            blockPosition = ConvertingWorld!.GetBlockPosition(testPosition);
            currentState = ConvertingWorld.GetBlockState(blockPosition);
            canPlace = currentState.Block.CanBeReplaced(currentState, BlockState);
        }

        if(!canPlace)
        {
            Destroy();
            lock(ConvertionStatusLocker)
            {
                ConvertionStatus = ConvertionStatus.Converted;
            }

            if(!BlockItem.TryFind(BlockState.Block, out var item))
                return;

            var dropPosition = Transform.Position + ModelBounds.Center;
            EntitySpawnConfig spawnConfig = new(new(dropPosition), ConvertingWorld);
            ItemStackEntity.Create(new Inventories.Stack<Item>(item), spawnConfig);
            return;
        }

        await ConvertingWorld!.SetBlockState(blockPosition, BlockState);
        Destroy();

        var blockCenterPosition = ConvertingWorld.GetBlockGlobalPosition(blockPosition) + MathV.UnitsInMeter / 2f;
        Sound.Play(currentState.Block.Properties.PlaceSound, blockCenterPosition);

        var currentBlockkState = ConvertingWorld.GetBlockState(blockPosition);
        if(currentBlockkState.Block is PhysicsBlock physicsBlock)
        {
            if(physicsBlock.ShouldConvertToEntity(ConvertingWorld, blockPosition, currentBlockkState))
                physicsBlock.ConvertToEntity(ConvertingWorld, blockPosition, currentBlockkState);
        }

        lock(ConvertionStatusLocker)
        {
            ConvertionStatus = ConvertionStatus.Converted;
        }
    }

    protected override void OnFixedUpdateChild()
    {
        lock(ConvertionStatusLocker)
        {
            if(ConvertionStatus == ConvertionStatus.ConvertionRequested)
                _ = ConvertToBlockOrDrop();
        }
    }

    protected override BinaryTag WriteAdditional()
    {
        CompoundTag tag = new();
        tag.Set("blockstate", BlockState ?? BlockState.Air);
        tag.Set("velocity", Rigidbody.Velocity);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        SetBlockState(BlockState.Read(compoundTag.GetTag("blockstate")));
        Rigidbody.Velocity = compoundTag.Get<Vector3>("velocity");
    }
}
