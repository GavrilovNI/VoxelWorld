using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
