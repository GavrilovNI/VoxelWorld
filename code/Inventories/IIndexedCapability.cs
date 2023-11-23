

namespace Sandcube.Inventories;

public interface IIndexedCapability<T> : ICapability<T> where T : class, IStack<T>
{
    int Size { get; }

    T Get(int index);

    bool TrySet(int index, T stack);
    bool TrySet(int index, T stack, int count) => TrySet(index, stack.WithCount(count));

    // TODO: uncomment(make default) when access T.Empty will be whitelisted
    void Remove(int index); // => TrySet(index, T.Empty);

    int GetMaxCount(int index) => 64;
    int GetMaxCount(int index, T stack) => GetMaxCount(index);

    int SetMax(int index, T stack, int count)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if(stack.IsEmpty || count <= 0)
        {
            Remove(index);
            return 0;
        }

        var possibleCount = Math.Min(count, GetMaxCount(index, stack));
        TrySet(index, stack, possibleCount);
        return possibleCount;
    }
    int SetMax(int index, T stack) => SetMax(index, stack, stack.Count);


    bool TryInsert(int index, T stack, int count, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var currentStack = Get(index);
        if(!currentStack.EqualsValue(stack))
            return false;

        var newCount = currentStack.Count + count;
        if(newCount > GetMaxCount(index, stack))
            return false;

        if(!simulate)
            TrySet(index, currentStack, newCount);
        return true;
    }
    bool TryInsert(int index, T stack, bool simulate = false) => TryInsert(index, stack, stack.Count, simulate);

    int InsertMax(int index, T stack, int count, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var currentStack = Get(index);
        if(!currentStack.EqualsValue(stack))
            return 0;

        var maxCountCanInsert = Math.Min(count, GetMaxCount(index, stack) - currentStack.Count);
        if(maxCountCanInsert <= 0)
            return 0;

        if(!simulate)
        {
            var newCount = currentStack.Count + maxCountCanInsert;
            TrySet(index, currentStack, newCount);
        }

        return maxCountCanInsert;
    }
    int InsertMax(int index, T stack, bool simulate = false) => InsertMax(index, stack, stack.Count, simulate);

    int ICapability<T>.InsertMax(T stack, int count, bool simulate)
    {
        int insertedCount = 0;
        InsertToValid(i => !Get(i).IsEmpty);
        if(count <= 0)
            return insertedCount;
        InsertToValid(i => Get(i).IsEmpty);

        void InsertToValid(Func<int, bool> isValid)
        {
            for(int i = 0; i < Size; ++i)
            {
                if(!isValid(i))
                    continue;
                var insertedCurrent = InsertMax(i, stack, count, simulate);
                insertedCount += insertedCurrent;
                count -= insertedCurrent;
                if(count <= 0)
                    return;
            }
        }

        return insertedCount;
    }


    T ExtractMax(int index, T? stackToExtract, int count, bool simulate = false);
    // TODO: uncomment(make default) when access T.Empty will be whitelisted
    /*{
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        bool limitedByStack = stackToExtract is not null;
        if(limitedByStack && stackToExtract!.IsEmpty)
            return T.Empty;

        var stack = Get(index);
        if(stack.IsEmpty || limitedByStack && !stackToExtract!.EqualsValue(stackToExtract))
            return T.Empty;

        if(stack.Count > count)
        {
            if(!simulate)
            {
                int leftCount = stack.Count - count;
                TrySet(index, stack, leftCount);
            }
            return stack.WithCount(count);
        }
        else
        {
            if(!simulate)
                Remove(index);

            return stack;
        }
    }*/

    T ExtractMax(int index, T stackToExtract, bool simulate = false) => ExtractMax(index, stackToExtract, Get(index).Count, simulate);

    T Extract(int index, bool simulate = false)
    {
        var stack = Get(index);
        if(stack.IsEmpty)
            return stack;

        if(!simulate)
            Remove(index);

        return stack;
    }

    bool TryExtract(int index, T? stackToExtract, int count, out T stack, bool simulate = false);
    // TODO: uncomment(make default) when access T.Empty will be whitelisted
    /*{
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if(count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        stack = Get(index);
        if(stack.Count < count)
            return false;

        stack = ExtractMax(index, stackToExtract, count, simulate);
        return true;
    }
    */

    bool TryExtract(int index, int count, out T stack, bool simulate = false) => TryExtract(index, Get(index), count, out stack, simulate);
    bool TryExtract(int index, T stackToExtract, out T stack, bool simulate = false) => TryExtract(index, stackToExtract, Get(index).Count, out stack, simulate);

    int ICapability<T>.ExtractMax(T stack, int count, bool simulate)
    {
        int extractedCount = 0;
        for(int i = 0; i < Size; ++i)
        {
            if(Get(i).IsEmpty)
                continue;
            var extractedCurrent = ExtractMax(i, stack, count, simulate);
            extractedCount += extractedCurrent.Count;
            count -= extractedCurrent.Count;
            if(count <= 0)
                return extractedCount;
        }
        return extractedCount;
    }
}
