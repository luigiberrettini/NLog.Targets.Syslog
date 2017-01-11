using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class TaskExtensions
    {
        public static Task<TResult> Then<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken token)
        {
            return task
                .ContinueWith(t =>
                {
                    var tcs = new TaskCompletionSource<TResult>();

                    if (t.IsCanceled)
                        tcs.SetCanceled();
                    else if (t.Exception != null) // t.IsFaulted is true
                        tcs.SetException(t.Exception.GetBaseException());
                    else
                        tcs.SetResult(continuationFunction(t));

                    return tcs.Task;
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                .Unwrap();
        }

        public static Task SafeFromAsync<TArg1, TArg2, TArg3>(this TaskFactory taskFactory, Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
        {
            try
            {
                return taskFactory.FromAsync(beginMethod, endMethod, arg1, arg2, arg3, state);
            }
            catch (Exception exception)
            {
                return new TaskCompletionSource<object>().FailedTask(exception);
            }
        }
    }
}