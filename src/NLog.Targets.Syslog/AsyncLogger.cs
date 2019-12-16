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
using NLog.Targets.Syslog.MessageStorage;
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
        private readonly LogEventInfo flushCompletionMarker;

        public AsyncLogger(Layout loggingLayout, EnforcementConfig enforcementConfig, MessageBuilder messageBuilder, MessageTransmitterConfig messageTransmitterConfig)
        {
            layout = loggingLayout;
            cts = new CancellationTokenSource();
            token = cts.Token;
            throttling = Throttling.FromConfig(enforcementConfig.Throttling);
            queue = NewBlockingCollection();
            buffer = new ByteArray(enforcementConfig.TruncateMessageTo);
            messageTransmitter = MessageTransmitter.FromConfig(messageTransmitterConfig);
            flushCompletionMarker = new LogEventInfo(LogLevel.Off, string.Empty, nameof(flushCompletionMarker));
            Task.Run(() => ProcessQueueAsync(messageBuilder));
        }

        public void Log(AsyncLogEventInfo asyncLogEvent)
        {
            throttling.Apply(queue.Count, timeout => Enqueue(asyncLogEvent, timeout));
        }

        public Task FlushAsync()
        {
            var flushTcs = new TaskCompletionSource<object>();
            Enqueue(flushCompletionMarker.WithContinuation(_ => flushTcs.SucceededTask()), Timeout.Infinite);
            return flushTcs.Task;
        }

        private BlockingCollection<AsyncLogEventInfo> NewBlockingCollection()
        {
            var throttlingLimit = throttling.Limit;

            return throttling.BoundedBlockingCollectionNeeded ?
                new BlockingCollection<AsyncLogEventInfo>(throttlingLimit) :
                new BlockingCollection<AsyncLogEventInfo>();
        }

        private void ProcessQueueAsync(MessageBuilder messageBuilder)
        {
            ProcessQueueAsync(messageBuilder, new TaskCompletionSource<object>())
                .ContinueWith(t =>
                {
                    InternalLogger.Warn(t.Exception?.GetBaseException(), "[Syslog] ProcessQueueAsync faulted within try");
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
                var logEventMsgSet = new LogEventMsgSet(asyncLogEventInfo, buffer, messageBuilder, messageTransmitter);
                logEventMsgSet
                    .Build(layout)
                    .SendAsync(token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                        {
                            InternalLogger.Debug("[Syslog] Task canceled");
                            tcs.SetCanceled();
                            return;
                        }
                        if (t.Exception != null) // t.IsFaulted is true
                            InternalLogger.Warn(t.Exception.GetBaseException(), "[Syslog] Task faulted");
                        else
                            InternalLogger.Debug("[Syslog] Successfully handled message '{0}'", logEventMsgSet);
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
            if (InternalLogger.IsDebugEnabled)
                InternalLogger.Debug("[Syslog] Enqueued '{0}'", asyncLogEventInfo.ToFormattedMessage());
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