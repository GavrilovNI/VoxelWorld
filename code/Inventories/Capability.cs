using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Inventories;

public abstract class Capability<T> : ICapability<T> where T : class, IStack<T>
{
    public abstract int InsertMax(T stack, int count, bool simulate = false);
    public int InsertMax(T stack, bool simulate = false) => InsertMax(stack, stack.Count, simulate);

    public virtual bool TryInsert(T stack, int count, bool simulate = false)
    {
        int maxCanInsert = InsertMax(stack, count, true);
        bool canInsertEnough = maxCanInsert >= count;
        if(canInsertEnough && !simulate)
            InsertMax(stack, count, false);
        return canInsertEnough;
    }
    public bool TryInsert(T stack, bool simulate = false) => TryInsert(stack, stack.Count, simulate);


    public abstract int ExtractMax(T stack, int count, bool simulate = false);
    public int ExtractMax(T stack, bool simulate = false) => ExtractMax(stack, stack.Count, simulate);

    public virtual bool TryExtract(T stack, int count, bool simulate = false)
    {
        int maxCanExtract = ExtractMax(stack, count, true);
        bool canExtractEnough = maxCanExtract >= count;
        if(canExtractEnough && !simulate)
            ExtractMax(stack, count, false);
        return canExtractEnough;
    }
    public bool TryExtract(T stack, bool simulate = false) => TryExtract(stack, stack.Count, simulate);

    public abstract IEnumerator<T> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
