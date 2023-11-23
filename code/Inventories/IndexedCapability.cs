

namespace Sandcube.Inventories;

public abstract class IndexedCapability<T> : Capability<T>, IIndexedCapability<T> where T : class, IStack<T>
{
    // TODO: remove when access T.Empty will be whitelisted
    [Obsolete("to remove when access to T.Empty will be whitelisted")]
    protected abstract T GetEmpty();

    public abstract int Size { get; }

    public abstract T Get(int index);
    protected abstract void Set(int index, T stack);
    protected void Set(int index, T stack, int count) => Set(index, stack.WithCount(count));

    public bool TrySet(int index, T stack)
    {
        if(index < 0 || index >= Size)
            return false;

        if(stack.IsEmpty)
        {
            Remove(index);
            return true;
        }

        if(stack.Count > GetMaxCount(index, stack))
            return false;

        Set(index, stack);
        return true;
    }
    public virtual bool TrySet(int index, T stack, int count) => TrySet(index, stack.WithCount(count));

#pragma warning disable CS0618 // Type or member is obsolete
    public virtual void Remove(int index) => Set(index, GetEmpty());
#pragma warning restore CS0618 // Type or member is obsolete

    public virtual int GetMaxCount(int index) => 64;
    public virtual int GetMaxCount(int index, T stack) => GetMaxCount(index);

    public virtual int SetMax(int index, T stack, int count)
    {
        if(index < 0 || index >= Size)
            return 0;

        if(stack.IsEmpty || count <= 0)
        {
            Remove(index);
            return 0;
        }

        var possibleCount = Math.Min(count, GetMaxCount(index, stack));
        Set(index, stack, possibleCount);
        return possibleCount;
    }
    public int SetMax(int index, T stack) => SetMax(index, stack, stack.Count);


    public virtual bool TryInsert(int index, T stack, int count, bool simulate = false)
    {
        if(count < 0 || index < 0 || index >= Size)
            return false;

        var currentStack = Get(index);
        if(!currentStack.EqualsValue(stack))
            return false;

        var newCount = currentStack.Count + count;
        if(newCount > GetMaxCount(index, stack))
            return false;

        if(!simulate)
            Set(index, currentStack, newCount);
        return true;
    }
    public bool TryInsert(int index, T stack, bool simulate = false) => TryInsert(index, stack, stack.Count, simulate);

    public virtual int InsertMax(int index, T stack, int count, bool simulate = false)
    {
        if(count <= 0 || index < 0 || index >= Size)
            return 0;

        var currentStack = Get(index);
        if(!currentStack.EqualsValue(stack))
            return 0;

        var maxCountCanInsert = Math.Min(count, GetMaxCount(index, stack) - currentStack.Count);
        if(maxCountCanInsert <= 0)
            return 0;

        if(!simulate)
        {
            var newCount = currentStack.Count + maxCountCanInsert;
            Set(index, currentStack, newCount);
        }

        return maxCountCanInsert;
    }
    public int InsertMax(int index, T stack, bool simulate = false) => InsertMax(index, stack, stack.Count, simulate);

    public override int InsertMax(T stack, int count, bool simulate = false)
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


    public virtual T ExtractMax(int index, T? stackToExtract, int count, bool simulate = false)
    {
        bool limitedByStack = stackToExtract is not null;

        if(count <= 0 || index < 0 || index >= Size || limitedByStack && stackToExtract!.IsEmpty)
#pragma warning disable CS0618 // Type or member is obsolete
            return GetEmpty();
#pragma warning restore CS0618 // Type or member is obsolete

        var stack = Get(index);
        if(stack.IsEmpty || limitedByStack && !stackToExtract!.EqualsValue(stackToExtract))
#pragma warning disable CS0618 // Type or member is obsolete
            return GetEmpty();
#pragma warning restore CS0618 // Type or member is obsolete

        if(stack.Count > count)
        {
            if(!simulate)
            {
                int leftCount = stack.Count - count;
                Set(index, stack, leftCount);
            }
            return stack.WithCount(count);
        }
        else
        {
            if(!simulate)
                Remove(index);

            return stack;
        }
    }

    public T ExtractMax(int index, T stackToExtract, bool simulate = false) => ExtractMax(index, stackToExtract, Get(index).Count, simulate);

    public virtual T Extract(int index, bool simulate = false)
    {
        var stack = Get(index);
        if(stack.IsEmpty)
            return stack;

        if(!simulate)
            Remove(index);

        return stack;
    }

    public virtual bool TryExtract(int index, T? stackToExtract, int count, out T stack, bool simulate = false)
    {
        if(count < 0 || index < 0 || index >= Size)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            stack = GetEmpty();
#pragma warning restore CS0618 // Type or member is obsolete
            return false;
        }

        stack = Get(index);
        if(stack.Count < count)
            return false;

        stack = ExtractMax(index, stackToExtract, count, simulate);
        return true;
    }

    public bool TryExtract(int index, int count, out T stack, bool simulate = false) => TryExtract(index, Get(index), count, out stack, simulate);
    public bool TryExtract(int index, T stackToExtract, out T stack, bool simulate = false) => TryExtract(index, stackToExtract, Get(index).Count, out stack, simulate);

    public override int ExtractMax(T stack, int count, bool simulate = false)
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
