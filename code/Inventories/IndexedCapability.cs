using Sandcube.Mth;
using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Inventories;

public abstract class IndexedCapability<T> : IIndexedCapability<T> where T : class, IStack<T>
{
    public abstract int Size { get; protected set; }

    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected abstract T GetEmpty();

    public virtual int GetStackLimit(int index) => DefaultValues.ItemStackLimit;
    public virtual int GetStackLimit(int index, T stack) => Math.Min(GetStackLimit(index), stack.ValueStackLimit);

    public abstract T Get(int index);

    public abstract int SetMax(int index, T stack, bool simulate = false);

    public virtual int InsertMax(int index, T stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var currentStack = Get(index);
        if(!currentStack.EqualsValue(stack))
            return 0;

        return SetMax(index, currentStack.Add(stack.Count), simulate);
    }

    public virtual T ExtractMax(int index, int count, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var stack = Get(index);
        if(stack.IsEmpty)
            return stack;

        var newStack = stack.Sub(count);
        if(this.TrySet(index, newStack, simulate))
            return stack.WithCount(count - newStack.Count);

        return GetEmpty();
    }


    public virtual int InsertMax(T stack, bool simulate)
    {
        var stackToInsert = stack;
        var insertedCount = insertMaxValid(stackToInsert, s => !s.IsEmpty, out stackToInsert);
        insertedCount += insertMaxValid(stackToInsert, s => s.IsEmpty, out _);
        return insertedCount;

        int insertMaxValid(T stack, Func<T, bool> validator, out T remainder)
        {
            remainder = stack;
            if(stack.IsEmpty)
                return 0;

            for(int i = 0; i < Size; ++i)
            {
                var currentStack = Get(i);
                if(!validator.Invoke(currentStack))
                    continue;

                var insertedCount = InsertMax(i, remainder, simulate);
                remainder = remainder.Sub(insertedCount);
                if(remainder.IsEmpty)
                    break;
            }
            return stack.Count - remainder.Count;
        }
    }

    public virtual int ExtractMax(T stack, bool simulate)
    {
        int extractedCount = 0;

        for(int i = 0; i < Size; ++i)
        {
            var currentStack = Get(i);
            if(!currentStack.EqualsValue(stack))
                continue;

            extractedCount += ExtractMax(i, currentStack.Count, simulate).Count;
        }

        return extractedCount;
    }


    private IEnumerator<T> DefaultEnumerator()
    {
        for (int i = 0; i < Size; ++i)
            yield return Get(i);
    }

    public virtual IEnumerator<T> GetEnumerator() => DefaultEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
