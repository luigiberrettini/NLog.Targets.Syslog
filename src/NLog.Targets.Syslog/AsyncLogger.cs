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
        private readonly Action<AsyncLogEventInfo, int> processWithTimeoutAction;
        private readonly Action<AsyncLogEventInfo> discardAction;

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
            processWithTimeoutAction = (asyncLogEventInfo, timeout) => Enqueue(asyncLogEventInfo, timeout);
            discardAction = asyncLogEventInfo => asyncLogEventInfo.Continuation(new InvalidOperationException($"Enqueue skipped"));
        }

        public void Log(AsyncLogEventInfo asyncLogEventInfo)
        {
            throttling.Apply(queue.Count, asyncLogEventInfo, processWithTimeoutAction, discardAction);
        }

        public Task FlushAsync()
        {
            var flushTcs = new TaskCompletionSource<object>();
            Enqueue(flushCompletionMarker.WithContinuation(_ => flushTcs.SetResult(null)), Timeout.Infinite);
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
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

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
            catch (OperationCanceledException)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
                return tcs.Task;
            }
        }

        private void Enqueue(AsyncLogEventInfo asyncLogEventInfo, int timeout)
        {
            bool enqueued = queue.TryAdd(asyncLogEventInfo, timeout, token);

            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug("[Syslog] {0} '{1}'", enqueued ? "Enqueued" : "Failed enqueuing", asyncLogEventInfo.ToFormattedMessage());
            }

            if (!enqueued)
                asyncLogEventInfo.Continuation(new InvalidOperationException($"Failed enqueuing"));
        }

        public void Dispose()
        {
            cts.Cancel();
            queue.CompleteAdding();
            queue.Dispose();
            messageTransmitter.Dispose();
            buffer.Dispose();
            cts.Dispose();
        }
    }
}
