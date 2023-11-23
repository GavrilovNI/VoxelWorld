using System.Collections.Generic;

namespace Sandcube.Inventories;

public interface ICapability<T> : IEnumerable<T> where T : class, IStack<T>
{
    int InsertMax(T stack, bool simulate = false);
    int ExtractMax(T stack, bool simulate = false);
}

public static class ICapabilityExtensions
{
    public static bool CanInsert<T>(this ICapability<T> capability, T stack) where T : class, IStack<T>
    {
        var insertedCount = capability.InsertMax(stack, true);
        return insertedCount == stack.Count;
    }

    public static bool TryInsert<T>(this ICapability<T> capability, T stack) where T : class, IStack<T>
    {
        bool canInsert = capability.CanInsert(stack);
        if(canInsert)
            capability.InsertMax(stack, false);
        return canInsert;
    }

    public static bool CanExtract<T>(this ICapability<T> capability, T stack) where T : class, IStack<T>
    {
        var extractedCount = capability.ExtractMax(stack, true);
        return extractedCount == stack.Count;
    }

    public static bool TryExtract<T>(this ICapability<T> capability, T stack) where T : class, IStack<T>
    {
        bool canExtract = capability.CanExtract(stack);
        if(canExtract)
            capability.ExtractMax(stack, false);
        return canExtract;
    }
}
