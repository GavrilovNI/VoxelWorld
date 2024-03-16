using System;

namespace VoxelWorld.Inventories;

public interface IIndexedCapability<T> : ICapability<T>, IReadOnlyIndexedCapability<T> where T : class, IStack<T>
{
    int SetMax(int index, T stack, bool simulate = false);

    int InsertMax(int index, T stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var currentStack = Get(index);
        if(!currentStack.IsEmpty && !currentStack.EqualsValue(stack))
            return 0;

        var stackToSet = stack.Add(currentStack.Count);
        int maxCanSet = SetMax(index, stackToSet, true);
        if(maxCanSet <= currentStack.Count)
            return 0;

        if(!simulate)
            SetMax(index, stackToSet, simulate);

        return maxCanSet - currentStack.Count;
    }

    T ExtractMax(int index, int count, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var stack = Get(index);
        if(stack.IsEmpty)
            return stack;

        var newStack = stack.Subtract(count);
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
                remainder = remainder.Subtract(insertedCount);
                if(remainder.IsEmpty)
                    break;
            }
            return stack.Count - remainder.Count;
        }
    }

    int ICapability<T>.ExtractMax(T stack, bool simulate)
    {
        int leftCount = stack.Count;
        if(leftCount == 0)
            return 0;

        for(int i = 0; i < Size; ++i)
        {
            var currentStack = Get(i);
            if(!currentStack.EqualsValue(stack))
                continue;

            leftCount -= ExtractMax(i, leftCount, simulate).Count;

            if(leftCount == 0)
                return stack.Count;
        }

        return stack.Count - leftCount;
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

    public static bool TryChange<T>(this IIndexedCapability<T> capability, int index, T stack, out T changedStack, bool simulate = false) where T : class, IStack<T>
    {
        var currentStack = capability.Get(index);

        if(!capability.TryExtract(index, currentStack.Count, out var extracted, false))
        {
            changedStack = stack;
            return false;
        }

        bool inserted = capability.TryInsert(index, stack, simulate);
        if(!inserted || simulate)
        {
            if(!capability.TrySet(index, extracted))
                Log.Warning($"Couldn't return stack {extracted} to capability {capability} at index {index} when failed to change with {stack}");

        }

        changedStack = inserted ? extracted : stack;
        return inserted;
    }

    public static bool TryChange<T>(this IIndexedCapability<T> @this, int thisIndex, IIndexedCapability<T> anotherCapability, int anotherIndex, bool simulate = false) where T : class, IStack<T>
    {
        var thisStack = @this.Get(thisIndex);
        var anotherStack = anotherCapability.Get(anotherIndex);

        if(@this.TryChange(thisIndex, anotherStack, out var _, true) && anotherCapability.TryChange(anotherIndex, thisStack, out var _, true))
        {
            if(!simulate)
            {
                if(!@this.TrySet(thisIndex, anotherStack))
                    Log.Warning($"Couldn't set stack {anotherStack} to capability {@this} when failed to changing with another capability");
                if(!anotherCapability.TrySet(anotherIndex, thisStack))
                    Log.Warning($"Couldn't set stack {thisStack} to capability {anotherCapability} when failed to changing with another capability");
            }

            return true;
        }

        return false;
    }

    public static int IndexOf<T>(this IIndexedCapability<T> capability, Func<T, bool> predicate) where T : class, IStack<T>
    {
        for(int i = 0; i < capability.Size; ++i)
        {
            var stack = capability.Get(i);
            if(predicate(stack))
                return i;
        }
        return -1;
    }
}
