using System;
using System.Collections.Generic;

namespace Sandcube.Inventories;

public abstract class StackInventory<T> : IndexedCapability<T> where T : class, IStack<T>
{
    private readonly Dictionary<int, T> _stacks = new();

    private int? _stacksHashCode = null;
    protected int StacksHashCode
    {
        get
        {
            if(_stacksHashCode == null)
            {
                _stacksHashCode = _stacks.Count;
                foreach(var (index, stack) in _stacks)
                    _stacksHashCode = HashCode.Combine(_stacksHashCode, index, stack);
            }
            return _stacksHashCode.Value;
        }
    }

    public override int Size { get; protected set; }


    public StackInventory(int size)
    {
        Size = size;
    }

    public override T Get(int index)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _stacks.GetValueOrDefault(index, GetEmpty());
    }

    public override int SetMax(int index, T stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var limit = GetSlotLimit(index, stack);
        var maxCountToSet = Math.Min(limit, stack.Count);
        if(!simulate)
        {
            _stacks[index] = stack.WithCount(maxCountToSet);
            _stacksHashCode = null;
        }

        return maxCountToSet;
    }

    public override int InsertMax(T stack, bool simulate)
    {
        var stackToInsert = stack;
        int insertedCount = 0;

        foreach(var currentStackEntry in _stacks)
        {
            insertedCount += InsertMax(currentStackEntry.Key, stackToInsert, simulate);
            stackToInsert = stackToInsert.Sub(insertedCount);
            if(stackToInsert.IsEmpty)
                return insertedCount;
        }

        for(int i = 0; i < Size; ++i)
        {
            var currentStack = Get(i);
            if(!currentStack.IsEmpty)
                continue;

            insertedCount += InsertMax(i, stackToInsert, simulate);
            stackToInsert = stackToInsert.Sub(insertedCount);
            if(stackToInsert.IsEmpty)
                return insertedCount;
        }

        return insertedCount;
    }

    public override IEnumerator<T> GetEnumerator() => _stacks.Values.GetEnumerator();

    public override int GetHashCode() => StacksHashCode;
}
