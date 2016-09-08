////////////////////////////////////////////////////////////////////////////////
//   NLog.Targets.Syslog
//   ------------------------------------------------------------------------
//   Copyright 2013 Jesper Hess Nielsen <jesper@graffen.dk>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Policies;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        private readonly CancellationTokenSource cts;
        private BlockingCollection<LogEventMsgSet> queue;

        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public Enforcement Enforcement { get; set; }

        /// <summary>The builder used to create messages according to RFCs</summary>
        public MessageBuildersFacade MessageBuilder { get; set; }

        /// <summary>The transmitter used to send messages to the Syslog server</summary>
        public MessageTransmittersFacade MessageTransmitter { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            cts = new CancellationTokenSource();
            Enforcement = new Enforcement();
            MessageBuilder = new MessageBuildersFacade();
            MessageTransmitter = new MessageTransmittersFacade();
        }

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            Enforcement.Throttling.EnsureAllowedValues();
            queue = NewBlockingCollection();
            MessageBuilder.Initialize(Enforcement);
            MessageTransmitter.Initialize();
            Task.Factory.StartNew(() => ProcessQueueAsync(cts.Token));
        }

        /// <summary>Writes a single event</summary>
        /// <param name="asyncLogEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to override it</remarks>
        protected override void Write(AsyncLogEventInfo asyncLogEvent)
        {
            MergeEventProperties(asyncLogEvent.LogEvent);
            var logEventMsgSet = new LogEventMsgSet(asyncLogEvent, MessageBuilder, MessageTransmitter);
            Enforcement.Throttling.Apply(queue.Count, delay => Enqueue(logEventMsgSet, delay));
        }

        private BlockingCollection<LogEventMsgSet> NewBlockingCollection()
        {
            var throttlingLimit = Enforcement.Throttling.Limit;

            return BoundedBlockingCollection ?
                new BlockingCollection<LogEventMsgSet>(throttlingLimit) :
                new BlockingCollection<LogEventMsgSet>();
        }

        private bool BoundedBlockingCollection
        {
            get
            {
                var throttlingStrategy = Enforcement.Throttling.Strategy;

                return throttlingStrategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
                       throttlingStrategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                       throttlingStrategy == ThrottlingStrategy.Block;
            }
        }

        private void ProcessQueueAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var logEventMsgSet = queue.Take(token);

            logEventMsgSet
                .Build(Layout)
                .SendAsync(token)
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

        private void Enqueue(LogEventMsgSet logEventMsgSet, int delay)
        {
            queue.TryAdd(logEventMsgSet, delay, cts.Token);
            InternalLogger.Debug($"Enqueued {logEventMsgSet}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    cts.Cancel();
                    queue.Dispose();
                    MessageBuilder.Dispose();
                    MessageTransmitter.Dispose();
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, $"{GetType().Name} dispose error");
                }
            }
            base.Dispose(disposing);
        }
    }
}