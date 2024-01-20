using Sandbox;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Sandcube.Threading;

public class ThreadHelpComponent : Component
{
    private record class ActionData(Action<CancellationToken> Action, TaskCompletionSource TaskCompletionSource,
        CancellationTokenSource CancellationTokenSource);
    private readonly ConcurrentQueue<ActionData> _actionsDataToRunInGameThread = new();
    private readonly object _destroyLocker = new();
    private bool _destroyed = false;

    protected sealed override void OnUpdate()
    {
        while(_actionsDataToRunInGameThread.TryDequeue(out var actionData))
        {
            var cancellationToken = actionData.CancellationTokenSource.Token;
            if(cancellationToken.IsCancellationRequested)
            {
                actionData.CancellationTokenSource.Dispose();
                actionData.TaskCompletionSource.SetCanceled();
                continue;
            }

            bool canceled = false;
            try
            {
                actionData.Action(cancellationToken);
            }
            catch(OperationCanceledException)
            {
                canceled = true;
            }

            if(canceled || cancellationToken.IsCancellationRequested)
                actionData.TaskCompletionSource.SetCanceled();
            else
                actionData.TaskCompletionSource.SetResult();

            actionData.CancellationTokenSource.Dispose();
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
        while(_actionsDataToRunInGameThread.TryDequeue(out var actionData))
        {
            actionData.CancellationTokenSource.Cancel();
            actionData.CancellationTokenSource.Dispose();
            actionData.TaskCompletionSource.SetCanceled();
        }
        OnDestroyInner();
    }

    protected virtual void OnDestroyInner() { }

    public Task RunInGameThread(Action<CancellationToken> action) => RunInGameThread(action, CancellationToken.None);

    public Task RunInGameThread(Action<CancellationToken> action, CancellationToken cancellationToken)
    {
        if(!IsValid)
            throw new TaskCanceledException();

        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TaskCompletionSource taskCompletionSource = new();
        ActionData actionData = new(action, taskCompletionSource, cancellationTokenSource);

        lock(_destroyLocker)
        {
            if(_destroyed)
            {
                cancellationTokenSource.Dispose();
                taskCompletionSource.SetCanceled(CancellationToken.None);
            }
            else
            {
                _actionsDataToRunInGameThread.Enqueue(actionData);
            }
        }
        return taskCompletionSource.Task;
    }
}
