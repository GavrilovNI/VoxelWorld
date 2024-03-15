using Sandbox;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values.Sandboxed;
using Sandcube.Items;
using Sandcube.Meshing;
using Sandcube.Mods.Base;
using System.Collections.Generic;

namespace Sandcube.Entities;

public class ItemStackEntity : Entity
{
    [Property] protected Rigidbody Rigidbody { get; set; } = null!;
    [Property] protected BoxCollider Collider { get; set; } = null!;

    protected List<ModelRenderer> Renderers { get; set; } = new();

    public Inventories.Stack<Item> ItemStack { get; private set; } = null!;
    public BBox ModelBounds { get; protected set; }


    public static ItemStackEntity Create(Inventories.Stack<Item> itemStack, in EntitySpawnConfig spawnConfig, Vector3 velocity = default)
    {
        ThreadSafe.AssertIsMainThread();
        var entity = (BaseMod.Instance!.Entities.ItemStack.CreateEntity(spawnConfig) as ItemStackEntity)!;
        entity.SetItemStack(itemStack);
        entity.Rigidbody.Velocity = velocity;
        return entity;
    }   


    protected override void OnAwakeChild()
    {
        if(ItemStack is null)
            SetItemStack(Inventories.Stack<Item>.Empty);
    }

    public virtual void SetItemStack(Inventories.Stack<Item> itemStack)
    {
        ThreadSafe.AssertIsMainThread();

        ItemStack = itemStack;
        if(itemStack.IsEmpty)
        {
            Collider.Enabled = false;
            foreach(var renderer in Renderers)
                renderer.Destroy();
            Renderers.Clear();
            Rigidbody.Enabled = false;
            ModelBounds = new BBox();
            return;
        }

        var game = SandcubeGame.Instance!;

        var item = ItemStack.Value;
        var itemModel = ItemStack.Value!.Model;

        var isTransparent = item is BlockItem blockItem && blockItem.Block.Properties.IsTransparent;
        var material = isTransparent ? game.TranslucentVoxelsMaterial : game.OpaqueVoxelsMaterial;

        ComplexMeshBuilder visualMeshBuilder = new ComplexMeshBuilder().Add(itemModel);
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
        ModelBounds = itemModel.Bounds;

        Collider.Center = itemModel.Bounds.Center;
        Collider.Scale = itemModel.Bounds.Size;
        Collider.Enabled = true;
        Rigidbody.Enabled = true;
    }

    protected override BinaryTag WriteAdditional()
    {
        CompoundTag tag = new();
        tag.Set("item_stack", ItemStack ?? Inventories.Stack<Item>.Empty);
        tag.Set("velocity", Rigidbody.Velocity);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        var itemStack = Inventories.ItemStack.Read(compoundTag.GetTag("item_stack"));
        SetItemStack(itemStack);
        Rigidbody.Velocity = compoundTag.Get<Vector3>("velocity");
    }

    protected override void DrawGizmosChild()
    {
        Gizmo.Hitbox.BBox(ModelBounds);
    }
}
