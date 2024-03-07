using Sandbox;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.IO.NamedBinaryTags.Values.Sandboxed;
using Sandcube.Items;
using Sandcube.Menus;
using Sandcube.Players;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Entities;

public class Player : Entity
{
    [Property] public GameObject Eye { get; private set; } = null!;
    [Property] public ItemDropper ItemDropper { get; private set; } = null!;
    [Property] public bool IsCreative { get; private set; } = false;
    [Property] public float ReachDistance { get; private set; } = 39.37f * 5;
    [Property] public PlayerInventory Inventory { get; private set; } = null!; // TODO: change to IPlayerInventory

    [Property] public ulong SteamId { get; private set; } = 0;

    public void SetSteamId(ulong steamId)
    {
        if(SteamId != 0)
            throw new InvalidOperationException($"steamId was already set");

        SteamId = steamId;
    }

    protected override void OnAwake()
    {
        Tags.Add("player");
    }

    public IMenu CreateInventoryMenu()
    {
        if(IsCreative)
        {
            return new CreativeInventoryMenu(new List<IIndexedCapability<Inventories.Stack<Item>>>()
            {
                Inventory.Hotbar
            }, this);
        }
        else
        {
            return new ItemCapabilitiesMenu(new List<IIndexedCapability<Inventories.Stack<Item>>>()
            {
                Inventory.SecondaryHand,
                Inventory.Main,
                Inventory.Hotbar
            }, this);
        }
    }

    protected override BinaryTag WriteAdditional()
    {
        CompoundTag tag = new();
        tag.Set("eye", Eye.Transform.Local);
        tag.Set("creative", IsCreative);
        tag.Set("reach_distance", ReachDistance);
        tag.Set("invetory", Inventory);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        Eye.Transform.Local = compoundTag.Get<Transform>("eye");
        IsCreative = compoundTag.Get<bool>("creative");
        ReachDistance = compoundTag.Get<float>("reach_distance");
        Inventory.Read(compoundTag.GetTag("invetory"));
    }

    protected override void WriteAdditional(BinaryWriter writer)
    {
        base.WriteAdditional(writer);
        writer.Write(Eye.Transform.Local);
        writer.Write(IsCreative);
        writer.Write(ReachDistance);
        Inventory.Write(writer);
    }

    protected override void ReadAdditional(BinaryReader reader)
    {
        base.ReadAdditional(reader);
        Eye.Transform.Local = reader.ReadTransform();
        IsCreative = reader.ReadBoolean();
        ReachDistance = reader.ReadSingle();
        Inventory.Read(reader);
    }
}
