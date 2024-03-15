using Sandbox;
using VoxelWorld.Interactions;
using VoxelWorld.Inventories;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Meshing;
using VoxelWorld.Mth;
using VoxelWorld.Registries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoxelWorld.Items;

public class Item : IRegisterable, IStackValue<Item>
{
    public ModedId Id { get; }
    public IMeshPart<ComplexVertex> Model { get; }
    public Texture Texture { get; }
    public int StackLimit { get; }

    public Item(in ModedId id, IMeshPart<ComplexVertex> model, Texture texture, int stackLimit)
    {
        Id = id;
        Model = model;
        Texture = texture;
        StackLimit = stackLimit;
    }

    public Item(in ModedId id, IMeshPart<ComplexVertex> model, Texture texture) : this(id, model, texture, DefaultValues.ItemStackLimit)
    {
    }

    public virtual int GetStackLimit() => StackLimit;

    public virtual Task<InteractionResult> OnAttack(ItemActionContext context) => Task.FromResult(InteractionResult.Pass);
    public virtual Task<InteractionResult> OnUse(ItemActionContext context) => Task.FromResult(InteractionResult.Pass);

    public override int GetHashCode() => HashCode.Combine(Id, StackLimit);

    public BinaryTag Write() => Id.Write();
    public static Item Read(BinaryTag tag)
    {
        var id = ModedId.Read(tag);

        var item = GameController.Instance!.Registries.GetRegistry<Item>().Get(id);
        if(item is null)
            throw new KeyNotFoundException($"Item with id {id} not found");

        return item;
    }
}
