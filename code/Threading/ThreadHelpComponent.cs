using Sandbox;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Sandcube.Data;

namespace Sandcube.Threading;

public class ThreadHelpComponent : Component
{
    #region Running Datas
    private abstract record class RunningData(CancellationTokenSource CancellationTokenSource)
    {
        public virtual void Cancel()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }

        // Call only in game thread
        public abstract void Run();
    }
    private record class TaskData(OneOf<Action<CancellationToken>, Func<CancellationToken, Task>> Task, TaskCompletionSource TaskCompletionSource,
        CancellationTokenSource CancellationTokenSource) : RunningData(CancellationTokenSource)
    {
        public override void Cancel()
        {
            base.Cancel();
            TaskCompletionSource.SetCanceled();
        }

        // Call only in game thread
        public override void Run()
        {
            if(CancellationTokenSource.IsCancellationRequested)
            {
                Cancel();
                return;
            }

            if(Task.Is<Action<CancellationToken>>(out var action))
            {
                if(CancellationTokenSource.IsCancellationRequested)
                    Cancel();
                action.Invoke(CancellationTokenSource.Token);
                if(CancellationTokenSource.IsCancellationRequested)
                    TaskCompletionSource.SetCanceled();
                else
                    TaskCompletionSource.SetResult();
                CancellationTokenSource.Dispose();
                return;
            }

            var taskSupplier = Task.As<Func<CancellationToken, Task>>()!;
            _ = taskSupplier.Invoke(CancellationTokenSource.Token).ContinueWith(t =>
            {
                if(CancellationTokenSource.IsCancellationRequested || !t.IsCompletedSuccessfully)
                    TaskCompletionSource.SetCanceled();
                else
                    TaskCompletionSource.SetResult();

                CancellationTokenSource.Dispose();
            });
        }
    }
    private record class ReturnableTaskData<T>(Func<CancellationToken, Task<T>> TaskSupplier, TaskCompletionSource<T> TaskCompletionSource,
        CancellationTokenSource CancellationTokenSource) : RunningData(CancellationTokenSource)
    {
        public override void Cancel()
        {
            base.Cancel();
            TaskCompletionSource.SetCanceled();
        }

        // Call only in game thread
        public override void Run()
        {
            if(CancellationTokenSource.IsCancellationRequested)
            {
                Cancel();
                return;
            }

            _ = TaskSupplier.Invoke(CancellationTokenSource.Token).ContinueWith(t =>
            {
                if(CancellationTokenSource.IsCancellationRequested || !t.IsCompletedSuccessfully)
                    TaskCompletionSource.TrySetCanceled();
                else
                    TaskCompletionSource.TrySetResult(t.Result);

                CancellationTokenSource.Dispose();
            });
        }
    }
    #endregion

    private readonly ConcurrentQueue<RunningData> _dataToRunInGameThread = new();
    private readonly object _destroyLocker = new();
    private bool _destroyed = false;


    // Thread safe
    public Task RunInGameThread(Action<CancellationToken> action) => Enqueue(action, CancellationToken.None);
    // Thread safe
    public Task RunInGameThread(Action<CancellationToken> action, CancellationToken cancellationToken) => Enqueue(action, cancellationToken);
    // Thread safe
    public Task RunInGameThread(Func<CancellationToken, Task> taskSupplier) => Enqueue(taskSupplier, CancellationToken.None);
    // Thread safe
    public Task RunInGameThread(Func<CancellationToken, Task> taskSupplier, CancellationToken cancellationToken) => Enqueue(taskSupplier, cancellationToken);
    // Thread safe
    public Task<T> RunInGameThread<T>(Func<CancellationToken, Task<T>> taskSupplier) => Enqueue(taskSupplier, CancellationToken.None);
    // Thread safe
    public Task<T> RunInGameThread<T>(Func<CancellationToken, Task<T>> taskSupplier, CancellationToken cancellationToken) => Enqueue(taskSupplier, cancellationToken);


    protected sealed override void OnUpdate()
    {
        while(_dataToRunInGameThread.TryDequeue(out var runningData))
        {
            var cancellationToken = runningData.CancellationTokenSource.Token;
            if(cancellationToken.IsCancellationRequested)
            {
                runningData.Cancel();
                continue;
            }

            runningData.Run();
        }

        OnUpdateInner();
    }

    protected virtual void OnUpdateInner() { }

    protected sealed override void OnDestroy()
    {
        lock(_destroyLocker)
        {
            _destroyed = true;
        }

        while(_dataToRunInGameThread.TryDequeue(out var runningData))
            runningData.Cancel();

        OnDestroyInner();
    }

    protected virtual void OnDestroyInner() { }

    // Thread safe
    private Task Enqueue(OneOf<Action<CancellationToken>, Func<CancellationToken, Task>> task,
        CancellationToken cancellationToken)
    {
        if(!IsValid)
            throw new TaskCanceledException();

        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TaskCompletionSource taskCompletionSource = new();
        TaskData taskData = new(task, taskCompletionSource, cancellationTokenSource);

        lock(_destroyLocker)
        {
            if(_destroyed)
            {
                cancellationTokenSource.Dispose();
                taskCompletionSource.SetCanceled(CancellationToken.None);
            }
            else
            {
                _dataToRunInGameThread.Enqueue(taskData);
            }
        }
        return taskCompletionSource.Task;
    }

    // Thread safe
    private Task<T> Enqueue<T>(Func<CancellationToken, Task<T>> task,
        CancellationToken cancellationToken)
    {
        if(!IsValid)
            throw new TaskCanceledException();

        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TaskCompletionSource<T> taskCompletionSource = new();
        ReturnableTaskData<T> taskData = new(task, taskCompletionSource, cancellationTokenSource);

        lock(_destroyLocker)
        {
            if(_destroyed)
            {
                cancellationTokenSource.Dispose();
                taskCompletionSource.SetCanceled(CancellationToken.None);
            }
            else
            {
                _dataToRunInGameThread.Enqueue(taskData);
            }
        }
        return taskCompletionSource.Task;
    }
}
