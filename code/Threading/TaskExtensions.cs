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

    public static async Task UnwrapWhitelisted(this Task<Task> task)
    {
        if(!task.IsCompleted)
            await task;

        await task.Result;
    }

    public static async Task<TResult> UnwrapWhitelisted<TResult>(this Task<Task<TResult>> task)
    {
        if(!task.IsCompleted)
            await task;

        return await task.Result;
    }

    public static async Task<TOut> ContinueWithOnMainThread<TIn, TOut>(this Task<TIn> task, Func<Task<TIn>, TOut> continuationFunction)
    {
        await GameTask.MainThread();
        return continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnMainThread<TOut>(this Task task, Func<Task, TOut> continuationFunction)
    {
        await GameTask.MainThread();
        return continuationFunction(task);
    }

    public static async Task ContinueWithOnMainThread<TIn>(this Task<TIn> task, Action<Task<TIn>> continuationFunction)
    {
        await GameTask.MainThread();
        continuationFunction(task);
    }

    public static async Task ContinueWithOnMainThread(this Task task, Action<Task> continuationFunction)
    {
        await GameTask.MainThread();
        continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnWorkerThread<TIn, TOut>(this Task<TIn> task, Func<Task<TIn>, TOut> continuationFunction)
    {
        await GameTask.WorkerThread();
        return continuationFunction(task);
    }

    public static async Task<TOut> ContinueWithOnWorkerThread<TOut>(this Task task, Func<Task, TOut> continuationFunction)
    {
        await GameTask.WorkerThread();
        return continuationFunction(task);
    }

    public static async Task ContinueWithOnWorkerThread<TIn>(this Task<TIn> task, Action<Task<TIn>> continuationFunction)
    {
        await GameTask.WorkerThread();
        continuationFunction(task);
    }

    public static async Task ContinueWithOnWorkerThread(this Task task, Action<Task> continuationFunction)
    {
        await GameTask.WorkerThread();
        continuationFunction(task);
    }
}
