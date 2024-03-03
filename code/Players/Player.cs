using Sandbox;
using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.IO;
using Sandcube.Items;
using Sandcube.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Players;

public class Player : Entity
{
    [Property] public GameObject Eye { get; private set; } = null!;
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
            });
        }
        else
        {
            return new ItemCapabilitiesMenu(new List<IIndexedCapability<Inventories.Stack<Item>>>()
            {
                Inventory.SecondaryHand,
                Inventory.Main,
                Inventory.Hotbar
            });
        }
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
