// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    internal class AsyncLogger
    {
        private readonly Layout layout;
        private readonly Throttling throttling;
        private readonly CancellationTokenSource cts;
        private readonly CancellationToken token;
        private readonly BlockingCollection<AsyncLogEventInfo> queue;
        private readonly ByteArray buffer;
        private readonly MessageTransmitter messageTransmitter;
        private TaskCompletionSource<object> flushTcs;
        private readonly AsyncLogEventInfo flushCompletionMarker;

        public AsyncLogger(Layout loggingLayout, EnforcementConfig enforcementConfig, MessageBuilder messageBuilder, MessageTransmitterConfig messageTransmitterConfig)
        {
            layout = loggingLayout;
            cts = new CancellationTokenSource();
            token = cts.Token;
            throttling = Throttling.FromConfig(enforcementConfig.Throttling);
            queue = NewBlockingCollection();
            buffer = new ByteArray(enforcementConfig.TruncateMessageTo);
            messageTransmitter = MessageTransmitter.FromConfig(messageTransmitterConfig);
            flushCompletionMarker = NewFlushCompletionMarker();
            Task.Run(() => ProcessQueueAsync(messageBuilder));
        }

        public void Log(AsyncLogEventInfo asyncLogEvent)
        {
            throttling.Apply(queue.Count, timeout => Enqueue(asyncLogEvent, timeout));
        }

        public Task FlushAsync()
        {
            Interlocked.Exchange(ref flushTcs, new TaskCompletionSource<object>());
            Enqueue(flushCompletionMarker, 0);
            return flushTcs.Task;
        }

        private BlockingCollection<AsyncLogEventInfo> NewBlockingCollection()
        {
            var throttlingLimit = throttling.Limit;

            return throttling.BoundedBlockingCollectionNeeded ?
                new BlockingCollection<AsyncLogEventInfo>(throttlingLimit) :
                new BlockingCollection<AsyncLogEventInfo>();
        }

        private AsyncLogEventInfo NewFlushCompletionMarker()
        {
            var logEvent = new LogEventInfo(LogLevel.Off, string.Empty, string.Empty);
            var asyncContinuation = new AsyncContinuation(exception => { });
            return new AsyncLogEventInfo(logEvent, asyncContinuation);
        }

        private void ProcessQueueAsync(MessageBuilder messageBuilder)
        {
            ProcessQueueAsync(messageBuilder, new TaskCompletionSource<object>())
                .ContinueWith(t =>
                {
                    InternalLogger.Warn(t.Exception?.GetBaseException(), "ProcessQueueAsync faulted within try");
                    ProcessQueueAsync(messageBuilder);
                }, token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
        }

        private Task ProcessQueueAsync(MessageBuilder messageBuilder, TaskCompletionSource<object> tcs)
        {
            if (token.IsCancellationRequested)
                return tcs.CanceledTask();

            try
            {
                var asyncLogEventInfo = queue.Take(token);
                if (asyncLogEventInfo == flushCompletionMarker)
                {
                    flushTcs.SetResult(null);
                    while (asyncLogEventInfo == flushCompletionMarker)
                        asyncLogEventInfo = queue.Take(token);
                }
                var logEventMsgSet = new LogEventMsgSet(asyncLogEventInfo, buffer, messageBuilder, messageTransmitter);

                logEventMsgSet
                    .Build(layout)
                    .SendAsync(token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                        {
                            InternalLogger.Debug("Task canceled");
                            tcs.SetCanceled();
                            return;
                        }
                        if (t.Exception != null) // t.IsFaulted is true
                            InternalLogger.Warn(t.Exception.GetBaseException(), "Task faulted");
                        else
                            InternalLogger.Debug("Successfully sent message '{0}'", logEventMsgSet);
                        ProcessQueueAsync(messageBuilder, tcs);
                    }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                return tcs.Task;
            }
            catch (Exception exception)
            {
                return tcs.FailedTask(exception);
            }
        }

        private void Enqueue(AsyncLogEventInfo asyncLogEventInfo, int timeout)
        {
            queue.TryAdd(asyncLogEventInfo, timeout, token);
            InternalLogger.Debug(() => $"Enqueued '{asyncLogEventInfo.ToFormattedMessage()}'");
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            queue.Dispose();
            buffer.Dispose();
            messageTransmitter.Dispose();
        }
    }
}