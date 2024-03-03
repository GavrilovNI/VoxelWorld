using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.IO;
using Sandcube.Meshing;
using Sandcube.Mth;
using Sandcube.Registries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sandcube.Items;

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

    public void Write(BinaryWriter writer)
    {
        writer.Write<ModedId>(Id);
    }

    public static Item Read(BinaryReader reader)
    {
        var id = ModedId.Read(reader);

        var item = SandcubeGame.Instance!.Registries.GetRegistry<Item>().Get(id);
        if(item is null)
            throw new KeyNotFoundException($"Item with id {id} not found");

        return item;
    }
}
