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

        public static Task CanceledTask(this TaskCompletionSource<object> tcs)
        {
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task SucceededTask(this TaskCompletionSource<object> tcs, Action action = null)
        {
            action?.Invoke();
            tcs.SetResult(null);
            return tcs.Task;
        }

        public static Task FailedTask(this TaskCompletionSource<object> tcs, Exception exception, Action<Exception> action = null)
        {
            action?.Invoke(exception);
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task SafeFromAsync<TArg1, TArg2, TArg3>(this TaskFactory taskFactory, Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
        {
            var tcs = new TaskCompletionSource<object>();
            try
            {
                beginMethod(arg1, arg2, arg3, Callback(endMethod, tcs), null);
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
            }
            return tcs.Task;
        }

        private static AsyncCallback Callback<TResult>(Action<IAsyncResult> endMethod, TaskCompletionSource<TResult> tcs)
        {
            return asyncResult =>
            {
                try
                {
                    endMethod(asyncResult);
                    tcs.SetResult(default(TResult));
                }
                catch (OperationCanceledException)
                {
                    tcs.SetCanceled();
                }
                catch (Exception exception)
                {
                    tcs.SetException(exception);
                }
            };
        }
    }
}