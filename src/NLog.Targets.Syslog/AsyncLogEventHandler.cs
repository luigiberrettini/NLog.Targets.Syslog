using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;

namespace NLog.Targets.Syslog
{
    internal class AsyncLogEventHandler
    {
        private readonly MessageBuildersFacade messageBuilder;
        private Layout layout;
        private readonly MessageTransmittersFacade messageTransmitter;
        private readonly Action<LogEventInfo> mergeEventProperties;
        private readonly ConcurrentQueue<LogEventMsgSet> queue;
        private readonly CancellationTokenSource cts;
        private volatile bool disposed;

        public AsyncLogEventHandler(SyslogTarget target, Action<LogEventInfo> mergeEventPropertiesAction)
        {
            messageBuilder = target.MessageBuilder;
            messageTransmitter = target.MessageTransmitter;
            mergeEventProperties = mergeEventPropertiesAction;
            queue = new ConcurrentQueue<LogEventMsgSet>();
            cts = new CancellationTokenSource();
        }

        public void Initialize(Layout targetLayout)
        {
            layout = targetLayout;
            ProcessQueueAsync(cts.Token);
        }

        public void Handle(AsyncLogEventInfo asyncLogEvent)
        {
            var logEventAndMessages = new LogEventMsgSet(asyncLogEvent, messageBuilder, layout);
            queue.Enqueue(logEventAndMessages);
            InternalLogger.Debug($"Enqueued {logEventAndMessages}");
        }

        private void ProcessQueueAsync(CancellationToken token)
        {
            if (disposed || token.IsCancellationRequested)
                return;

            LogEventMsgSet logEventMsgSet;
            var sendOrDelayTask = queue.TryDequeue(out logEventMsgSet) ?
                SendMsgSetAsync(logEventMsgSet, token) :
                Task.Delay(messageTransmitter.RetryInterval, token);

            sendOrDelayTask
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        InternalLogger.Debug("Task canceled");
                    else if (t.Exception != null) // t.IsFaulted is true
                        InternalLogger.Debug(t.Exception.GetBaseException(), "Task faulted with exception");
                    else
                        ProcessQueueAsync(token);
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        private Task SendMsgSetAsync(LogEventMsgSet logEventMsgSet, CancellationToken token)
        {
            logEventMsgSet.BuildLogEntries();
            return SendMsgSetAsync(logEventMsgSet, token, new TaskCompletionSource<object>());
        }

        private Task SendMsgSetAsync(LogEventMsgSet logEventMsgSet, CancellationToken token, TaskCompletionSource<object> tcs, Exception exception = null)
        {
            if (disposed || token.IsCancellationRequested)
                return CancelledTcsTask(tcs);

            // All messages have been dequeued and either all messages have been sent or
            // an exception has occurred and has been propagated till here
            // (no message was sent after it)
            if (logEventMsgSet.HasNoMessages)
                return MessagesDequeuedTcsTask(logEventMsgSet, tcs, exception);

            messageTransmitter
                .SendMessageAsync(logEventMsgSet.NextMessage, token)
                .Then(t => SendMsgSetAsync(logEventMsgSet, token, tcs, t.Exception), token);

            return tcs.Task;
        }

        private static Task CancelledTcsTask(TaskCompletionSource<object> tcs)
        {
            tcs.SetCanceled();
            return tcs.Task;
        }

        private static Task MessagesDequeuedTcsTask(LogEventMsgSet logEventMsgSet, TaskCompletionSource<object> tcs, Exception exception)
        {
            InternalLogger.Debug($"Dequeued {logEventMsgSet}");

            if (exception != null)
            {
                logEventMsgSet.OnNoMessages(exception.GetBaseException());
                tcs.SetException(exception);
            }
            else
            {
                logEventMsgSet.OnNoMessages();
                tcs.SetResult(null);
            }

            return tcs.Task;
        }

        public void Dispose()
        {
            try
            {
                if (disposed)
                    return;
                disposed = true;
                messageBuilder.Dispose();
                messageTransmitter.Dispose();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, $"{GetType().Name} dispose error");
            }
        }
    }
}