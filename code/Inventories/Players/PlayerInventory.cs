using Sandbox;
using Sandcube.Interactions;
using Sandcube.Items;
using System;

namespace Sandcube.Inventories.Players;

public class PlayerInventory : Component, IPlayerInventory
{
    public IIndexedCapability<Stack<Item>> Main { get; }
    public IIndexedCapability<Stack<Item>> Hotbar { get; }

    public IIndexedCapability<Stack<Item>> SecondaryHand { get; }

    protected int _mainHandIndex = 0;
    public virtual int MainHandIndex
    {
        get => _mainHandIndex;
        set => _mainHandIndex = Math.Clamp(value, 0, Hotbar.Size - 1);
    }

    public PlayerInventory()
    {
        Hotbar = new ItemStackInventory(9);
        Main = new ItemStackInventory(9 * 3);
        SecondaryHand = new ItemStackInventory(1);
    }

    public virtual Stack<Item> GetHandItem(HandType handType)
    {
        switch(handType)
        {
            case HandType.Main:
                var mainHandIndex = MainHandIndex;
                if(mainHandIndex < 0)
                    return Stack<Item>.Empty;
                return Hotbar.Get(mainHandIndex);
            case HandType.Secondary:
                return SecondaryHand.Get(0);
            default:
                throw new ArgumentException($"{nameof(HandType)} {handType} not supported");
        }
    }

    public virtual bool TrySetHandItem(HandType handType, Stack<Item> stack, bool simulate = false)
    {
        switch(handType)
        {
            case HandType.Main:
                var mainHandIndex = MainHandIndex;
                if(mainHandIndex < 0)
                    return false;
                return Hotbar.TrySet(mainHandIndex, stack, simulate);
            case HandType.Secondary:
                return SecondaryHand.TrySet(0, stack, simulate);
            default:
                throw new ArgumentException($"{nameof(HandType)} {handType} not supported");
        }
    }

    public override int GetHashCode() => HashCode.Combine(Hotbar, SecondaryHand, Main, _mainHandIndex);
}
