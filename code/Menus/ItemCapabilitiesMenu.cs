using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Menus;

using ItemStack = VoxelWorld.Inventories.Stack<Item>;

public class ItemCapabilitiesMenu : IMenu
{
    protected List<IIndexedCapability<ItemStack>> Capabilities { get; set; }
    public ItemStack TakenStack { get; protected set; } = ItemStack.Empty;

    protected IIndexedCapability<ItemStack>? CapabilityTakenFrom = null;
    protected int IndexTakenFrom = -1;

    public Player Player { get; }

    public int CapabilitiesCount => Capabilities.Count;

    public ItemCapabilitiesMenu(Player player)
    {
        Capabilities = new List<IIndexedCapability<ItemStack>>();
        Player = player;
    }

    public ItemCapabilitiesMenu(IEnumerable<IIndexedCapability<ItemStack>> capabilities, Player player)
    {
        Capabilities = new List<IIndexedCapability<ItemStack>>(capabilities);
        Player = player;
    }

    public IReadOnlyIndexedCapability<ItemStack> GetCapability(int capabilityIndex) => Capabilities[capabilityIndex];

    protected IIndexedCapability<ItemStack>? FindEditableCapability(IReadOnlyIndexedCapability<ItemStack> capability, ref int slotIndex)
    {
        IIndexedCapability<ItemStack>? result = Capabilities.FirstOrDefault(x => x == capability, null)!;
        if(result is not null)
            return result;

        if(capability is IndexedCapabilityPart<ItemStack> part)
        {
            int newIndex = part.GetParentIndex(slotIndex);
            result = FindEditableCapability(part.Capability, ref newIndex);
            if(result is not null)
            {
                slotIndex = newIndex;
                return result;
            }
        }

        return capability as IIndexedCapability<ItemStack>;
    }

    public virtual bool TakeStack(IReadOnlyIndexedCapability<ItemStack> capability, int slotIndex, int maxCount)
    {
        var changableCapability = FindEditableCapability(capability, ref slotIndex);
        if(changableCapability is null)
            throw new ArgumentException($"Couldn't find editable capability of {capability}", nameof(capability));

        if(maxCount <= 0)
            return false;

        var clickedStack = changableCapability.Get(slotIndex);
        if(clickedStack.IsEmpty)
            return false;

        if(!TakenStack.IsEmpty && !TakenStack.EqualsValue(clickedStack))
            return false;

        maxCount = Math.Min(maxCount, clickedStack.ValueStackLimit - TakenStack.Count);

        var extracted = changableCapability.ExtractMax(slotIndex, maxCount);
        if(!extracted.IsEmpty)
            TakenStack = extracted.Add(TakenStack.Count);

        bool took = !extracted.IsEmpty;
        if(took)
        {
            CapabilityTakenFrom = changableCapability;
            IndexTakenFrom = slotIndex;
        }
        return took;
    }

    public virtual bool PlaceStack(IReadOnlyIndexedCapability<ItemStack> capability, int slotIndex, int maxCount)
    {
        var changableCapability = FindEditableCapability(capability, ref slotIndex);
        if(changableCapability is null)
            throw new ArgumentException($"Couldn't find editable capability of {capability}", nameof(capability));

        if(TakenStack.IsEmpty)
            return false;
        if(maxCount <= 0)
            return false;


        var clickedStack = changableCapability.Get(slotIndex);

        if(clickedStack.IsEmpty || clickedStack.EqualsValue(TakenStack))
        {
            var inserted = changableCapability.InsertMax(slotIndex, TakenStack.WithCount(maxCount));
            TakenStack = TakenStack.Subtract(inserted);
            return inserted > 0;
        }
        else
        {
            bool changed = changableCapability.TryChange(slotIndex, TakenStack, out var takenStack);
            if(changed)
                TakenStack = takenStack;
            return changed;
        }
    }

    public virtual void ReturnTakenStack()
    {
        if(!TakenStack.IsEmpty)
        {
            var inserted = CapabilityTakenFrom!.InsertMax(IndexTakenFrom, TakenStack);
            TakenStack = TakenStack.Subtract(inserted);

            if(!TakenStack.IsEmpty)
            {
                var capabilityToInsert = new CombinedCapability<ItemStack>(Capabilities);
                inserted = capabilityToInsert.InsertMax(TakenStack);
                TakenStack = TakenStack.Subtract(inserted);
                if(!TakenStack.IsEmpty)
                    DropTakenStack(TakenStack.Count);
            }
        }

        CapabilityTakenFrom = null;
        IndexTakenFrom = -1;
    }

    public virtual void DropTakenStack(int maxCount)
    {
        maxCount = Math.Min(TakenStack.Count, maxCount);

        var stackToDrop = TakenStack.WithCount(maxCount);
        if(stackToDrop.IsEmpty)
            return;

        TakenStack = TakenStack.Subtract(maxCount);

        Player.ItemDropper.Drop(stackToDrop);
    }


    public virtual bool IsStillValid() => true;

    public virtual GameObject CreateScreen()
    {
        var gameObject = new GameObject();
        var screen = gameObject.Components.Create<SimpleItemCapabilitiesScreen>();
        screen.Menu = this;
        return gameObject;
    }

    public override int GetHashCode()
    {
        var hashCode = TakenStack.GetHashCode();
        foreach(var capability in Capabilities)
            hashCode = HashCode.Combine(hashCode, capability);
        return hashCode;
    }
}
