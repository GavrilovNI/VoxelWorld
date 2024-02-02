using System;

namespace Sandcube.Inventories;

public interface IIndexedCapability<T> : ICapability<T> where T : class, IStack<T>
{
    int Size { get; }

    T Get(int index);
    int SetMax(int index, T stack, bool simulate = false);

    int InsertMax(int index, T stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var currentStack = Get(index);
        if(!currentStack.IsEmpty && !currentStack.EqualsValue(stack))
            return 0;

        return SetMax(index, stack.Add(currentStack.Count), simulate);
    }

    T ExtractMax(int index, int count, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var stack = Get(index);
        if(stack.IsEmpty)
            return stack;

        var newStack = stack.Sub(count);
        if(this.TrySet(index, newStack, simulate))
            return stack.WithCount(stack.Count - newStack.Count);

        return stack.WithCount(0); // TODO: use T.Empty when will be whitelisted
    }


    int ICapability<T>.InsertMax(T stack, bool simulate)
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

    int ICapability<T>.ExtractMax(T stack, bool simulate)
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
}

public static class IIndexedCapabilityExtensions
{
    public static int ExtractMax<T>(this IIndexedCapability<T> capability, int index, T stack, bool simulate) where T : class, IStack<T>
    {
        var currentStack = capability.Get(index);
        if(!currentStack.EqualsValue(stack))
            return 0;

        capability.SetMax(index, stack.WithCount(0), simulate); // TODO: use T.Empty when will be whitelisted
        return currentStack.Count;
    }

    public static bool TrySet<T>(this IIndexedCapability<T> capability, int index, T stack, bool simulate = false) where T : class, IStack<T>
    {
        int maxCanSet = capability.SetMax(index, stack, true);
        bool canSetEnough = maxCanSet >= stack.Count;
        if(canSetEnough && !simulate)
            capability.SetMax(index, stack, false);
        return canSetEnough;
    }

    public static bool TryInsert<T>(this IIndexedCapability<T> capability, int index, T stack, bool simulate = false) where T : class, IStack<T>
    {
        int maxCanInsert = capability.InsertMax(index, stack, true);
        bool canInsertEnough = maxCanInsert >= stack.Count;
        if(canInsertEnough && !simulate)
            capability.InsertMax(index, stack, false);
        return canInsertEnough;
    }

    public static bool TryExtract<T>(this IIndexedCapability<T> capability, int index, int count, out T extracted, bool simulate = false) where T : class, IStack<T>
    {
        extracted = capability.ExtractMax(index, count, true);
        bool canExtractEnough = extracted.Count >= count;
        if(canExtractEnough && !simulate)
            extracted = capability.ExtractMax(index, count, false);
        return canExtractEnough;
    }

    public static T Extract<T>(this IIndexedCapability<T> capability, int index, bool simulate = false) where T : class, IStack<T> =>
        capability.ExtractMax(index, int.MaxValue, simulate);
}
