using Sandcube.Interactions;
using Sandcube.Items;
using Sandcube.Mth;

namespace Sandcube.Inventories.Players;

public class PlayerInventory : BaseComponent, IPlayerInventory
{
    protected readonly IIndexedCapability<Stack<Item>> _hotBar;
    protected Stack<Item> _secondaryHand = Stack<Item>.Empty;

    protected int _mainHandIndex = 0;
    public virtual int MainHandIndex
    {
        get => _mainHandIndex;
        set => _mainHandIndex = Math.Clamp(value, 0, _hotBar.Size - 1);
    }

    public virtual int HotbarSize => _hotBar.Size;

    public PlayerInventory() : this(9)
    {
    }

    public PlayerInventory(int hotbarSize)
    {
        if(hotbarSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(hotbarSize));

        _hotBar = new ItemStackInventory(hotbarSize);
    }

    public virtual int GetHotbarStackLimit(int index) => DefaultValues.ItemStackLimit;
    public virtual int GetHotbarStackLimit(int index, Stack<Item> stack) => Math.Min(GetHotbarStackLimit(index), stack.ValueStackLimit);

    public virtual bool CanSetHotbarItem(int index, Stack<Item> stack) => stack.Count <= GetHotbarStackLimit(index, stack);
    public Stack<Item> GetHotbarItem(int index) => _hotBar.Get(index);
    public bool TrySetHotbarItem(int index, Stack<Item> stack)
    {
        if(!CanSetHotbarItem(index, stack))
            return false;

        return _hotBar.TrySet(index, stack);
    }


    public virtual int GetHandStackLimit(HandType handType) =>
        handType == HandType.Main ? GetHotbarStackLimit(MainHandIndex) : DefaultValues.ItemStackLimit;
    public virtual int GetHandStackLimit(HandType handType, Stack<Item> stack)
    {
        int result = GetHandStackLimit(handType);
        if(handType == HandType.Main)
            result = Math.Min(result, GetHotbarStackLimit(MainHandIndex, stack));
        return result;
    }

    public virtual bool CanSetHandItem(HandType handType, Stack<Item> stack)
    {
        if(stack.Count > GetHandStackLimit(handType, stack))
            return false;

        return handType != HandType.Main || CanSetHotbarItem(MainHandIndex, stack);
    }

    public virtual Stack<Item> GetHandItem(HandType handType)
    {
        switch(handType)
        {
            case HandType.Main:
                var mainHandIndex = MainHandIndex;
                if(mainHandIndex < 0)
                    return Stack<Item>.Empty;
                return _hotBar.Get(mainHandIndex);
            case HandType.Secondary:
                return _secondaryHand;
            default:
                throw new ArgumentException($"{nameof(HandType)} {handType} not supported");
        }
    }

    public virtual bool TrySetHandItem(HandType handType, Stack<Item> stack)
    {
        if(!CanSetHandItem(handType, stack))
            return false;

        switch(handType)
        {
            case HandType.Main:
                var mainHandIndex = MainHandIndex;
                if(mainHandIndex < 0)
                    return false;
                return _hotBar.TrySet(mainHandIndex, stack);
            case HandType.Secondary:
                _secondaryHand = stack;
                return true;
            default:
                throw new ArgumentException($"{nameof(HandType)} {handType} not supported");
        }
    }
}
