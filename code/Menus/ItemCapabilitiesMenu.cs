using Sandbox;
using Sandcube.Inventories;
using Sandcube.Items;
using Sandcube.Players;
using Sandcube.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Menus;

using ItemStack = Sandcube.Inventories.Stack<Item>;

public class ItemCapabilitiesMenu : IMenu
{
    protected List<IIndexedCapability<ItemStack>> Capabilities { get; set; }
    public ItemStack TakenStack { get; protected set; } = ItemStack.Empty;

    protected IIndexedCapability<ItemStack>? CapabilityTakenFrom = null;
    protected int IndexTakenFrom = -1;

    public int CapabilitiesCount => Capabilities.Count;

    public ItemCapabilitiesMenu()
    {
        Capabilities = new List<IIndexedCapability<ItemStack>>();
    }

    public ItemCapabilitiesMenu(IEnumerable<IIndexedCapability<ItemStack>> capabilities)
    {
        Capabilities = new List<IIndexedCapability<ItemStack>>(capabilities);
    }

    public IReadOnlyIndexedCapability<ItemStack> GetCapability(int capabilityIndex) => Capabilities[capabilityIndex];
    protected IIndexedCapability<ItemStack>? FindCapability(IReadOnlyIndexedCapability<ItemStack> capability) => Capabilities.FirstOrDefault(x => x == capability, null);

    public virtual bool TakeStack(IReadOnlyIndexedCapability<ItemStack> capability, int slotIndex, int maxCount)
    {
        var changableCapability = FindCapability(capability);
        if(changableCapability is null)
            throw new ArgumentException($"Capability {capability} is not part of menu", nameof(capability));

        if(!TakenStack.IsEmpty)
            return false;
        if(maxCount <= 0)
            return false;

        var clickedStack = changableCapability.Get(slotIndex);
        if(clickedStack.IsEmpty)
            return false;

        TakenStack = changableCapability.ExtractMax(slotIndex, maxCount);
        bool took = !TakenStack.IsEmpty;
        if(took)
        {
            CapabilityTakenFrom = changableCapability;
            IndexTakenFrom = slotIndex;
        }
        return took;
    }

    public virtual bool PlaceStack(IReadOnlyIndexedCapability<ItemStack> capability, int slotIndex, int maxCount)
    {
        var changableCapability = FindCapability(capability);
        if(changableCapability is null)
            throw new ArgumentException($"Capability {capability} is not part of menu", nameof(capability));

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

        Log.Info($"Drop: {stackToDrop}"); //TODO
    }


    public virtual bool IsStillValid(Player player) => true;

    public virtual GameObject CreateScreen(Player player)
    {
        var gameObject = new GameObject();
        var screen = gameObject.Components.Create<ItemCapabilitiesScreen>();
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
