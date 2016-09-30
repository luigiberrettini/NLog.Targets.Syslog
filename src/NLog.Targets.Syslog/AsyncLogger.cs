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
        private readonly int id;
        private readonly Throttling throttling;
        private readonly CancellationToken token;
        private readonly BlockingCollection<AsyncLogEventInfo> queue;
        private readonly MessageBuilder messageBuilder;
        private readonly MessageTransmitter messageTransmitter;

        public AsyncLogger(Layout loggingLayout, Configuration config, int loggerId, CancellationToken ct)
        {
            layout = loggingLayout;
            id = loggerId;
            token = ct;
            throttling = Throttling.FromConfig(config.Enforcement.Throttling);
            queue = NewBlockingCollection();
            messageBuilder = MessageBuilder.FromConfig(config.MessageCreation, config.Enforcement);
            messageTransmitter = MessageTransmitter.FromConfig(config.MessageSend);
            Task.Factory.StartNew(ProcessQueueAsync, token);
        }

        public void Log(AsyncLogEventInfo asyncLogEvent)
        {
            throttling.Apply(queue.Count, delay => Enqueue(asyncLogEvent, delay));
        }

        private BlockingCollection<AsyncLogEventInfo> NewBlockingCollection()
        {
            var throttlingLimit = throttling.Limit;

            return throttling.BoundedBlockingCollectionNeeded ?
                new BlockingCollection<AsyncLogEventInfo>(throttlingLimit) :
                new BlockingCollection<AsyncLogEventInfo>();
        }

        private void ProcessQueueAsync()
        {
            if (token.IsCancellationRequested)
                return;

            var logEventMsgSet = new LogEventMsgSet(queue.Take(token), messageBuilder, messageTransmitter);

            logEventMsgSet
                .Build(layout)
                .SendAsync(token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        InternalLogger.Debug($"AsyncLogger {id} - Task canceled");
                        return;
                    }
                    if (t.Exception != null) // t.IsFaulted is true
                        InternalLogger.Debug(t.Exception.GetBaseException(), $"AsyncLogger {id} - Task faulted");
                    else
                        InternalLogger.Debug($"AsyncLogger {id} - Successfully sent the dequeued message set '{logEventMsgSet}'");
                    ProcessQueueAsync();
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        private void Enqueue(AsyncLogEventInfo asyncLogEventInfo, int delay)
        {
            queue.TryAdd(asyncLogEventInfo, delay, token);
            InternalLogger.Debug($"Enqueued '{asyncLogEventInfo.ToFormattedMessage()}'");
        }

        public void Dispose()
        {
            queue.Dispose();
            messageBuilder.Dispose();
            messageTransmitter.Dispose();
        }
    }
}