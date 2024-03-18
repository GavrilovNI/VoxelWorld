using Sandbox;
using VoxelWorld.Inventories;
using VoxelWorld.Inventories.Players;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Items;
using VoxelWorld.Menus;
using VoxelWorld.Players;
using System;
using System.Collections.Generic;
using VoxelWorld.Worlds;
using VoxelWorld.Registries;
using VoxelWorld.Mods.Base;

namespace VoxelWorld.Entities;

public class Player : Entity
{
    [Property] public GameObject Eye { get; private set; } = null!;
    [Property] public CameraComponent Camera { get; private set; } = null!;
    [Property] public ItemDropper ItemDropper { get; private set; } = null!;
    [Property] public bool IsCreative { get; private set; } = false;
    [Property] public float ReachDistance { get; private set; } = 39.37f * 5;
    [Property] public PlayerInventory Inventory { get; private set; } = null!; // TODO: change to IPlayerInventory

    [Property] public ulong SteamId { get; private set; } = 0;

    public void SetSteamId(ulong steamId)
    {
        if(SteamId == steamId)
            return;

        if(SteamId != 0)
            throw new InvalidOperationException($"steamId was already set");

        SteamId = steamId;
    }

    protected override void OnAwakeChild()
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
        tag.Set("steam_id", SteamId);
        tag.Set("eye_rotation", Eye.Transform.LocalRotation);
        tag.Set("camera_rotation", Camera.Transform.LocalRotation);
        tag.Set("creative", IsCreative);
        tag.Set("reach_distance", ReachDistance);
        tag.Set("invetory", Inventory);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        if(SteamId == 0)
            SteamId = compoundTag.Get<ulong>("steam_id");
        Eye.Transform.LocalRotation = compoundTag.Get<Rotation>("eye_rotation");
        Camera.Transform.LocalRotation = compoundTag.Get<Rotation>("camera_rotation");
        IsCreative = compoundTag.Get<bool>("creative");
        ReachDistance = compoundTag.Get<float>("reach_distance");
        Inventory.Read(compoundTag.GetTag("invetory"));
    }


    public static bool TryReadPlayer(BinaryTag tag, IWorldAccessor? world, out Player player, bool enable = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        return TryReadPlayer(compoundTag, compoundTag.Get<ulong>("steam_id"), world, out player, enable);
    }

    public static bool TryReadPlayer(BinaryTag tag, out Player player, bool enable = true, bool readWithWorld = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        return TryReadPlayer(compoundTag, compoundTag.Get<ulong>("steam_id"), out player, enable, readWithWorld);
    }

    public static bool TryReadPlayer(BinaryTag tag, ulong steamId, out Player player, bool enable = true, bool readWithWorld = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        World? world = null;
        if(readWithWorld)
        {
            var worldId = ModedId.Read(compoundTag.GetTag("world_id"));
            if(!Worlds.World.TryFind(worldId, out world))
            {
                player = null!;
                return false;
            }
        }

        return TryReadPlayer(compoundTag, steamId, world, out player, enable);
    }

    public static bool TryReadPlayer(BinaryTag tag, ulong steamId, IWorldAccessor? world, out Player player, bool enable = true)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        var typeId = ModedId.Read(compoundTag.GetTag("type_id"));
        if(typeId != BaseMod.Instance!.Entities.Player.Id)
        {
            player = null!;
            return false;
        }

        var dataTag = compoundTag.GetTag<CompoundTag>("data");
        ulong? oldSteamId = dataTag.Get<ulong>("steam_id", null);
        dataTag.Set("steam_id", steamId);
        var entity = Read(compoundTag, null, false);
        dataTag.Set("steam_id", oldSteamId);

        if(entity is not Player playerEntity)
        {
            entity.Destroy();
            player = null!;
            return false;
        }
        playerEntity.ChangeWorld(world);

        playerEntity.Enabled = enable;
        player = playerEntity;
        return true;
    }
}
