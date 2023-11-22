using System.Collections.Generic;

namespace Sandcube.Inventories;

public interface ICapability<T> : IEnumerable<T> where T : class, IStack<T>
{
    int InsertMax(T stack, int count, bool simulate = false);
    int InsertMax(T stack, bool simulate = false) => InsertMax(stack, stack.Count, simulate);

    bool TryInsert(T stack, int count, bool simulate = false)
    {
        int maxCanInsert = InsertMax(stack, count, true);
        bool canInsertEnough = maxCanInsert >= count;
        if(canInsertEnough && !simulate)
            InsertMax(stack, count, false);
        return canInsertEnough;
    }
    bool TryInsert(T stack, bool simulate = false) => TryInsert(stack, stack.Count, simulate);


    int ExtractMax(T stack, int count, bool simulate = false);
    int ExtractMax(T stack, bool simulate = false) => ExtractMax(stack, stack.Count, simulate);

    bool TryExtract(T stack, int count, bool simulate = false)
    {
        int maxCanExtract = ExtractMax(stack, count, true);
        bool canExtractEnough = maxCanExtract >= count;
        if(canExtractEnough && !simulate)
            ExtractMax(stack, count, false);
        return canExtractEnough;
    }
    bool TryExtract(T stack, bool simulate = false) => TryExtract(stack, stack.Count, simulate);
}
