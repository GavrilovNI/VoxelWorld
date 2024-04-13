using Sandbox;
using VoxelWorld.Interactions;
using VoxelWorld.Inventories;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using VoxelWorld.Registries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace VoxelWorld.Items;

public class Item : IRegisterable, IStackValue<Item>
{
    public ModedId Id { get; }
    public ImmutableArray<Model> Models { get; }
    public Texture Texture { get; }
    public int StackLimit { get; }
    public bool IsFlatModel { get; } //TODO: remove

    public Item(in ModedId id, Model[] models, Texture texture, int stackLimit, bool isFlatModel = false)
    {
        Id = id;
        Models = ImmutableArray.Create(models);
        Texture = texture;
        StackLimit = stackLimit;
        IsFlatModel = isFlatModel;
    }

    public Item(in ModedId id, Model[] models, Texture texture, bool isFlatModel = false) : this(id, models, texture, DefaultValues.ItemStackLimit, isFlatModel)
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
