using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.IO;
using Sandcube.Mth;
using Sandcube.Registries;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Items;

public class Item : IRegisterable, IStackValue<Item>
{
    public ModedId Id { get; }
    public Texture Texture { get; }
    public int StackLimit { get; }

    public Item(in ModedId id, Texture texture, int stackLimit)
    {
        Id = id;
        Texture = texture;
        StackLimit = stackLimit;
    }

    public Item(in ModedId id, Texture texture) : this(id, texture, DefaultValues.ItemStackLimit)
    {
    }

    public virtual int GetStackLimit() => StackLimit;

    public virtual InteractionResult OnAttack(in ItemActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnUse(in ItemActionContext context) => InteractionResult.Pass;

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
