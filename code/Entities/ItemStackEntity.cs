using Sandbox;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Items;
using VoxelWorld.Mods.Base;
using VoxelWorld.Physics;
using VoxelWorld.Rendering;

namespace VoxelWorld.Entities;

public class ItemStackEntity : Entity
{
    [Property] protected RigidbodyWithMemory Rigidbody { get; set; } = null!;
    [Property] protected BoxCollider Collider { get; set; } = null!;

    [Property, RequireComponent] protected UnlimitedModelRenderer ModelRenderer { get; set; } = null!;

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
            ModelRenderer.SceneObjectAttrubutes.Set("color", GameController.Instance!.ItemsTextureMap.Texture);
    }

    public virtual void SetItemStack(Inventories.Stack<Item> itemStack)
    {
        ThreadSafe.AssertIsMainThread();

        ItemStack = itemStack;
        if(itemStack.IsEmpty)
        {
            Collider.Enabled = false;
            ModelRenderer.Enabled = false;
            Rigidbody.Enabled = false;
            ModelBounds = new BBox();
            return;
        }

        var itemModels = ItemStack.Value!.Models;
        ModelRenderer.SetModels(itemModels);
        ModelBounds = ModelRenderer.ModelBounds;
        if(Started && ItemStack.Value!.IsFlatModel)
            ModelRenderer.SceneObjectAttrubutes.Set("color", GameController.Instance!.ItemsTextureMap.Texture);

        Collider.Center = ModelBounds.Center;
        Collider.Scale = ModelBounds.Size;

        ModelRenderer.Enabled = true;
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
