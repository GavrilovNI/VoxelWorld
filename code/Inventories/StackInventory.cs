using System.Collections.Generic;

namespace Sandcube.Inventories;

public abstract class StackInventory<T> : IndexedCapability<T> where T : class, IStack<T>
{
    private readonly Dictionary<int, T> _stacks = new();
    public override int Size { get; }

    public StackInventory(int size)
    {
        Size = size;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public override T Get(int index) => _stacks.GetValueOrDefault(index, GetEmpty());
#pragma warning restore CS0618 // Type or member is obsolete

    protected override void Set(int index, T stack)
    {
        if(stack.IsEmpty)
            _stacks.Remove(index);
        else
            _stacks[index] = stack;
    }

    public override void Remove(int index) => _stacks.Remove(index);

    public override int InsertMax(T stack, int count, bool simulate)
    {
        int insertedCount = 0;
        foreach(var currentStackIndex in _stacks.Keys)
        {
            var insertedCurrent = InsertMax(currentStackIndex, stack, count, simulate);
            insertedCount += insertedCurrent;
            count -= insertedCurrent;
            if(count <= 0)
                return insertedCount;
        }
        for(int i = 0; i < Size; ++i)
        {
            if(!Get(i).IsEmpty)
                continue;
            var insertedCurrent = InsertMax(i, stack, count, simulate);
            insertedCount += insertedCurrent;
            count -= insertedCurrent;
            if(count <= 0)
                return insertedCount;
        }
        return insertedCount;
    }

    public override int ExtractMax(T stack, int count, bool simulate = false)
    {
        int extractedCount = 0;
        foreach(var currentStackIndex in _stacks.Keys)
        {
            var extractedCurrent = ExtractMax(currentStackIndex, stack, count, simulate);
            extractedCount += extractedCurrent.Count;
            count -= extractedCurrent.Count;
            if(count <= 0)
                return extractedCount;
        }
        return extractedCount;
    }

    public override IEnumerator<T> GetEnumerator() => _stacks.Values.GetEnumerator();
}
