// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class TaskExtensions
    {
        public static Task Then(this Task task, Func<Task, Task> continuationFunction, CancellationToken token)
        {
            return task
                .Then<Task>(continuationFunction, token)
                .Unwrap();
        }

        public static Task<TResult> Then<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken token)
        {
            return task
                .ContinueWith((t, c) =>
                {
                    var tcs = new TaskCompletionSource<TResult>();

                    var exception = t.Exception;
                    if (token.IsCancellationRequested || t.IsCanceled)
                        tcs.SetCanceled();
                    else if (exception != null) // t.IsFaulted is true
                        tcs.SetException(exception.GetBaseException());
                    else
                        tcs.SetResult(((Func<Task, TResult>)c).Invoke(t));

                    return tcs.Task;
                }, continuationFunction, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                .Unwrap();
        }
    }
}