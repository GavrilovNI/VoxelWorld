using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Inventories.Players;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Players;

public class ItemPickuper : Component, Component.ITriggerListener
{
    [Property] protected PlayerInventory Inventory { get; set; } = null!;
    [Property] protected float PickupTime { get; set; } = 1f;

    protected readonly Dictionary<ItemStackEntity, TimeSince> Entities = new();

    public void OnTriggerEnter(Collider other)
    {
        if(other.Components.TryGet<ItemStackEntity>(out var entity))
            Entities.Add(entity, 0);
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.Components.TryGet<ItemStackEntity>(out var entity))
            Entities.Remove(entity);
    }

    protected virtual bool TryPickup(ItemStackEntity entity)
    {
        var itemStack = entity.ItemStack;
        int inserted = Inventory.Combined.InsertMax(itemStack);

        bool pickedUpAll = inserted >= itemStack.Count;

        if(pickedUpAll)
            entity.Destroy();
        else
            entity.SetItemStack(itemStack.Subtract(inserted));

        return pickedUpAll;
    }

    protected override void OnUpdate()
    {
        HashSet<ItemStackEntity> entitiesToRemove = new();

        foreach(var (entity, timeSince) in Entities.ToList())
        {
            if(!entity.IsValid)
            {
                entitiesToRemove.Add(entity);
                continue;
            }

            if(timeSince < PickupTime)
                continue;

            if(TryPickup(entity))
                entitiesToRemove.Add(entity);
            else
                Entities[entity] = 0;
        }

        foreach(var entity in entitiesToRemove)
            Entities.Remove(entity);
    }
}
