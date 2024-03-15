using Sandbox;
using System;
using System.Threading.Tasks;

namespace VoxelWorld.Threading;

public static class TaskSourceExtensions
{
    public static async Task RunInMainThreadAsync(this TaskSource taskSource, Action action)
    {
        if(!ThreadSafe.IsMainThread)
        {
            taskSource.CancelIfInvalid();
            await taskSource.MainThread();
        }

        taskSource.CancelIfInvalid();
        action();
    }

    public static async Task<T> RunInMainThreadAsync<T>(this TaskSource taskSource, Func<T> func)
    {
        if(!ThreadSafe.IsMainThread)
        {
            taskSource.CancelIfInvalid();
            await taskSource.MainThread();
        }

        taskSource.CancelIfInvalid();
        return func();
    }

    public static async Task RunInMainThreadAsync(this TaskSource taskSource, Func<Task> func)
    {
        if(!ThreadSafe.IsMainThread)
        {
            taskSource.CancelIfInvalid();
            await taskSource.MainThread();
        }

        taskSource.CancelIfInvalid();
        await func();
    }

    public static async Task<T> RunInMainThreadAsync<T>(this TaskSource taskSource, Func<Task<T>> func)
    {
        if(!ThreadSafe.IsMainThread)
        {
            taskSource.CancelIfInvalid();
            await taskSource.MainThread();
        }

        taskSource.CancelIfInvalid();
        return await func();
    }


    private static void CancelIfInvalid(this TaskSource taskSource)
    {
        if(!taskSource.IsValid)
            throw new TaskCanceledException();
    }
}
