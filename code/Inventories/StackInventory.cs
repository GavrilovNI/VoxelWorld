using Sandcube.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Inventories;

//TODO: implement IBinaryStaticReadable when T.Read will be whitelisted
public abstract class StackInventory<T> : IndexedCapability<T>, IBinaryWritable, ISaveStatusMarkable where T : class, IStack<T>
{
    private readonly Dictionary<int, T> _stacks = new();
    public readonly int SlotLimit;

    private int? _stacksHashCode = null;
    protected int StacksHashCode
    {
        get
        {
            if(_stacksHashCode == null)
            {
                _stacksHashCode = HashCode.Combine(SlotLimit, _stacks.Count);
                foreach(var (index, stack) in _stacks)
                    _stacksHashCode = HashCode.Combine(_stacksHashCode, index, stack);
            }
            return _stacksHashCode.Value;
        }
    }

    public override int Size { get; }

    public bool IsSaved { get; protected set; }

    public StackInventory(int size, int slotLimit = int.MaxValue)
    {
        Size = size;
        SlotLimit = slotLimit;
    }

    public override T Get(int index)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _stacks.GetValueOrDefault(index, GetEmpty());
    }

    public override int GetSlotLimit(int index) => SlotLimit;

    protected virtual void Set(int index, T stack)
    {
        _stacks[index] = stack;
        _stacksHashCode = null;
        IsSaved = false;
    }

    public override int SetMax(int index, T stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var limit = GetSlotLimit(index, stack);
        var maxCountToSet = Math.Min(limit, stack.Count);
        if(!simulate)
            Set(index, stack);

        return maxCountToSet;
    }

    public override int InsertMax(T stack, bool simulate)
    {
        var stackToInsert = stack;
        int insertedCount = 0;

        foreach(var currentStackEntry in _stacks)
        {
            insertedCount += InsertMax(currentStackEntry.Key, stackToInsert, simulate);
            stackToInsert = stackToInsert.Subtract(insertedCount);
            if(stackToInsert.IsEmpty)
                return insertedCount;
        }

        for(int i = 0; i < Size; ++i)
        {
            var currentStack = Get(i);
            if(!currentStack.IsEmpty)
                continue;

            insertedCount += InsertMax(i, stackToInsert, simulate);
            stackToInsert = stackToInsert.Subtract(insertedCount);
            if(stackToInsert.IsEmpty)
                return insertedCount;
        }

        return insertedCount;
    }

    public override IEnumerator<T> GetEnumerator() => _stacks.Values.GetEnumerator();

    public override int GetHashCode() => StacksHashCode;

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(SlotLimit);
        writer.Write(Size);
        for(int i = 0; i < Size; ++i)
            writer.Write(Get(i));
    }

    public void MarkSaved() => IsSaved = true;
}
