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
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        private volatile bool toBeInited;
        private CancellationTokenSource cts;
        private BlockingCollection<AsyncLogEventInfo> queue;
        private Throttling throttling;
        private MessageBuilder[] messageBuilders;
        private MessageTransmitter[] messageTransmitters;

        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public EnforcementConfig Enforcement { get; set; }

        /// <summary>The builder used to create messages according to RFCs</summary>
        public MessageBuilderConfig MessageCreation { get; set; }

        /// <summary>The transmitter used to send messages to the Syslog server</summary>
        public MessageTransmitterConfig MessageSend { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            toBeInited = true;
            Enforcement = new EnforcementConfig();
            MessageCreation = new MessageBuilderConfig();
            MessageSend = new MessageTransmitterConfig();
        }

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            CleanupOnConfigReload();
            Initialize();
            StartBackgroundProcessing();
            toBeInited = false;
        }

        /// <summary>Writes a single event</summary>
        /// <param name="asyncLogEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to override it</remarks>
        protected override void Write(AsyncLogEventInfo asyncLogEvent)
        {
            MergeEventProperties(asyncLogEvent.LogEvent);
            throttling.Apply(queue.Count, delay => Enqueue(asyncLogEvent, delay));
        }

        private void CleanupOnConfigReload()
        {
            if (toBeInited)
                return;
            DisposeDependencies();
        }

        private void Initialize()
        {
            cts = new CancellationTokenSource();
            queue = NewBlockingCollection();
            Enforcement.EnsureAllowedValues();
            throttling = Throttling.FromConfig(Enforcement.Throttling);
            messageBuilders = MessageBuilder.FromConfig(Enforcement.MessageProcessors, MessageCreation, Enforcement);
            messageTransmitters = MessageTransmitter.FromConfig(Enforcement.MessageProcessors, MessageSend);
        }

        private BlockingCollection<AsyncLogEventInfo> NewBlockingCollection()
        {
            var throttlingLimit = Enforcement.Throttling.Limit;

            return BoundedBlockingCollection ?
                new BlockingCollection<AsyncLogEventInfo>(throttlingLimit) :
                new BlockingCollection<AsyncLogEventInfo>();
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

        private void StartBackgroundProcessing()
        {
            Enforcement.MessageProcessors.ForEach(StartMessageProcessor);
        }

        private void StartMessageProcessor(int id)
        {
            Task.Factory.StartNew(() => ProcessQueueAsync(id, cts.Token));
        }

        private void ProcessQueueAsync(int msgProcessorId, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var logEventMsgSet = new LogEventMsgSet(queue.Take(token), messageBuilders[msgProcessorId], messageTransmitters[msgProcessorId]);

            logEventMsgSet
                .Build(Layout)
                .SendAsync(token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        InternalLogger.Debug($"Message processor {msgProcessorId} - Task canceled");
                        return;
                    }
                    if (t.Exception != null) // t.IsFaulted is true
                        InternalLogger.Debug(t.Exception.GetBaseException(), $"Message processor {msgProcessorId} - Task faulted");
                    else
                        InternalLogger.Debug($"Message processor {msgProcessorId} - Successfully sent the dequeued message set '{logEventMsgSet}'");
                    ProcessQueueAsync(msgProcessorId, token);
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        private void Enqueue(AsyncLogEventInfo asyncLogEventInfo, int delay)
        {
            queue.TryAdd(asyncLogEventInfo, delay, cts.Token);
            InternalLogger.Debug($"Enqueued '{asyncLogEventInfo.LogEvent.FormattedMessage}'");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeDependencies();
            base.Dispose(disposing);
        }

        private void DisposeDependencies()
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
                queue.Dispose();
                Enforcement.MessageProcessors.ForEach(i =>
                {
                    messageBuilders[i].Dispose();
                    messageTransmitters[i].Dispose();
                });
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, $"{GetType().Name} dispose error");
            }
        }
    }
}