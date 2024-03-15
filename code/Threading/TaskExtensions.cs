using Sandbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Threading;

public static class TaskExtensions
{
    public static void ThrowIfNotCompletedSuccessfully(this Task task)
    {
        if(!task.IsCompleted)
            throw new InvalidOperationException("task wasn't completed");

        if(task.IsCompletedSuccessfully)
            return;

        //if(task.IsCanceled)
        //    throw new TaskCanceledException();

        //if(task.IsFaulted)
        //    throw new Exception("Task faulted");
        throw new Exception("Task canceled or faulted");
    }

    public static async Task<TOut> ConvertResult<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> convertor)
    {
        var result = await task;
        return convertor(result);
    }

    public static async Task<bool> TryAwait(this Task task)
    {
        try
        {
            await task;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<TOut> ContinueWithOnMainThread<TIn, TOut>(this Task<TIn> task, Func<Task<TIn>, TOut> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.MainThread();
        return continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnMainThread<TOut>(this Task task, Func<Task, TOut> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.MainThread();
        return continuationFunction(task);
    }

    public static async Task ContinueWithOnMainThread<TIn>(this Task<TIn> task, Action<Task<TIn>> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.MainThread();
        continuationFunction(task);
    }

    public static async Task ContinueWithOnMainThread(this Task task, Action<Task> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.MainThread();
        continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnWorkerThread<TIn, TOut>(this Task<TIn> task, Func<Task<TIn>, TOut> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.WorkerThread();
        return continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnWorkerThread<TOut>(this Task task, Func<Task, TOut> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.WorkerThread();
        return continuationFunction(task);
    }

    public static async Task ContinueWithOnWorkerThread<TIn>(this Task<TIn> task, Action<Task<TIn>> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.WorkerThread();
        continuationFunction(task);
    }

    public static async Task ContinueWithOnWorkerThread(this Task task, Action<Task> continuationFunction)
    {
        await task.TryAwait();
        await GameTask.WorkerThread();
        continuationFunction(task);
    }
}
