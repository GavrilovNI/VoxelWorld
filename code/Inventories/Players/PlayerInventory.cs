using Sandbox;
using Sandcube.Interactions;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Items;
using System;
using System.IO;

namespace Sandcube.Inventories.Players;

public class PlayerInventory : Component, IPlayerInventory, INbtWritable, INbtReadable
{
    public ItemStackInventory Main { get; private set; }
    public ItemStackInventory Hotbar { get; private set; }
    public ItemStackInventory SecondaryHand { get; private set; }

    public IIndexedCapability<Stack<Item>> Combined => new CombinedIndexedCapability<Stack<Item>>(Hotbar, Main, SecondaryHand);

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

    public virtual BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("main", Main);
        tag.Set("hotbar", Hotbar);
        tag.Set("secondary_hand", SecondaryHand);
        tag.Set("main_hand_index", MainHandIndex);
        return tag;
    }

    public virtual void Read(BinaryTag tag)
    {
        CompoundTag compoundTag = (CompoundTag)tag;
        Main = ItemStackInventory.Read(compoundTag.GetTag("main"));
        Hotbar = ItemStackInventory.Read(compoundTag.GetTag("hotbar"));
        SecondaryHand = ItemStackInventory.Read(compoundTag.GetTag("secondary_hand"));
        MainHandIndex = compoundTag.Get<int>("main_hand_index");
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(Main);
        writer.Write(Hotbar);
        writer.Write(SecondaryHand);
        writer.Write(MainHandIndex);
    }

    public virtual void Read(BinaryReader reader)
    {
        Main = ItemStackInventory.Read(reader);
        Hotbar = ItemStackInventory.Read(reader);
        SecondaryHand = ItemStackInventory.Read(reader);
        MainHandIndex = reader.ReadInt32();
    }
}
