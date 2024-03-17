using Sandbox;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Items;
using VoxelWorld.Meshing;
using VoxelWorld.Mods.Base;
using System.Collections.Generic;
using System.Linq;
using VoxelWorld.Inventories;

namespace VoxelWorld.Entities;

public class ItemStackEntity : Entity
{
    [Property] protected Rigidbody Rigidbody { get; set; } = null!;
    [Property] protected BoxCollider Collider { get; set; } = null!;

    [Property] protected ModelRenderer Renderer { get; set; } = null!;

    public Inventories.Stack<Item> ItemStack { get; private set; } = null!;
    public BBox ModelBounds { get; protected set; }

    protected bool Started = false;


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

    protected override void OnStartChild()
    {
        Started = true;
        if(!ItemStack.IsEmpty && ItemStack.Value!.IsFlatModel)
            Renderer.SceneObject.Attributes.Set("color", GameController.Instance!.ItemsTextureMap.Texture);
    }

    public virtual void SetItemStack(Inventories.Stack<Item> itemStack)
    {
        ThreadSafe.AssertIsMainThread();

        ItemStack = itemStack;
        if(itemStack.IsEmpty)
        {
            Collider.Enabled = false;
            Renderer.Enabled = false;
            Rigidbody.Enabled = false;
            ModelBounds = new BBox();
            return;
        }

        var itemModel = ItemStack.Value!.Model;
        Renderer.Model = itemModel;
        ModelBounds = itemModel.Bounds;
        if(Started && ItemStack.Value!.IsFlatModel)
            Renderer.SceneObject.Attributes.Set("color", GameController.Instance!.ItemsTextureMap.Texture);

        Collider.Center = itemModel.Bounds.Center;
        Collider.Scale = itemModel.Bounds.Size;
        Renderer.Enabled = true;
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
