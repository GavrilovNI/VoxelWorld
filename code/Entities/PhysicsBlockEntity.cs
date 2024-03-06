using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values.Sandboxed;
using Sandcube.Items;
using Sandcube.Meshing;
using Sandcube.Meshing.Blocks;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sandcube.Entities;

public class PhysicsBlockEntity : Entity, Component.ICollisionListener
{
    [Property] protected Rigidbody Rigidbody { get; set; } = null!;
    [Property] protected ModelCollider Collider { get; set; } = null!;

    protected List<ModelRenderer> Renderers { get; set; } = new();

    public BBox ModelBounds { get; protected set; }
    public BlockState BlockState { get; private set; } = null!;

    protected override void OnAwake()
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

        var game = SandcubeGame.Instance!;
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

    protected override void DrawGizmos()
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
        if(Direction.ClosestTo(other.Contact.Normal) == Direction.Down)
            _ = ConvertToBlockOrDrop();
    }

    public virtual void OnCollisionUpdate(Collision other) { }
    public virtual void OnCollisionStop(CollisionStop other) { }

    public virtual async Task ConvertToBlockOrDrop()
    {
        Destroy();
        Vector3 testPosition = Transform.Position + ModelBounds.Center.WithZ(ModelBounds.Mins.z);

        var blockPosition = World!.GetBlockPosition(testPosition);
        var state = World.GetBlockState(blockPosition);
        if(!state.Block.CanBeReplaced(state, BlockState))
        {
            if(!BlockItem.TryFind(BlockState.Block, out var item))
                return;

            var dropPosition = Transform.Position + ModelBounds.Center;
            EntitySpawnConfig spawnConfig = new(new(dropPosition), World);
            ItemStackEntity.Create(new Inventories.Stack<Item>(item), spawnConfig);
            return;
        }

        await World.SetBlockState(blockPosition, BlockState);

        var currentBlockkState = World.GetBlockState(blockPosition);
        if(currentBlockkState.Block is PhysicsBlock physicsBlock)
        {
            if(physicsBlock.ShouldConvertToEntity(World, blockPosition, currentBlockkState))
                physicsBlock.ConvertToEntity(World, blockPosition, currentBlockkState);
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
        CompoundTag compoundTag = (CompoundTag)tag;
        SetBlockState(BlockState.Read(compoundTag.GetTag("blockstate")));
        Rigidbody.Velocity = compoundTag.Get<Vector3>("velocity");
    }

    protected override void WriteAdditional(BinaryWriter writer)
    {
        base.WriteAdditional(writer);
        writer.Write(BlockState ?? BlockState.Air);
        writer.Write(Rigidbody.Velocity);
    }

    protected override void ReadAdditional(BinaryReader reader)
    {
        base.ReadAdditional(reader);
        var blockState = BlockState.Read(reader);
        SetBlockState(blockState);
        Rigidbody.Velocity = reader.ReadVector3();
    }
}
