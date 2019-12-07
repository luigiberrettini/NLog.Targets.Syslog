// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : AsyncTaskTarget
    {
        private MessageBuilder messageBuilder;
        private MessageTransmitter messageTransmitter;
        private ByteArray reusableBuffer;

        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public EnforcementConfig Enforcement { get; set; }

        /// <summary>The settings used to create messages according to RFCs</summary>
        public MessageBuilderConfig MessageCreation { get; set; }

        /// <summary>The settings used to send messages to the Syslog server</summary>
        public MessageTransmitterConfig MessageSend { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            Enforcement = new EnforcementConfig();
            MessageCreation = new MessageBuilderConfig();
            MessageSend = new MessageTransmitterConfig();
        }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            Enforcement.PropertyChanged += Init;
            MessageCreation.PropertyChanged += Init;
            MessageSend.PropertyChanged += Init;
            Init();
            base.InitializeTarget();
        }

        private void Init(object sender = null, PropertyChangedEventArgs eventArgs = null)
        {
            if (Enforcement.Throttling.Strategy != ThrottlingStrategy.None)
            {
                QueueLimit = Math.Max(Enforcement.Throttling.Limit, 2);
                if (Enforcement.Throttling.Strategy == ThrottlingStrategy.Block)
                    OverflowAction = Wrappers.AsyncTargetWrapperOverflowAction.Block;
                else if (Enforcement.Throttling.Strategy == ThrottlingStrategy.DeferForFixedTime
                    || Enforcement.Throttling.Strategy == ThrottlingStrategy.DeferForPercentageTime)
                    OverflowAction = Wrappers.AsyncTargetWrapperOverflowAction.Grow;
            }
                
            messageBuilder = MessageBuilder.FromConfig(MessageCreation, Enforcement);
            messageTransmitter = MessageTransmitter.FromConfig(MessageSend);
        }

        /// <inheritdoc />
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            if (logEvent.Level == LogLevel.Off)
                return;

            var buffer = Interlocked.Exchange(ref reusableBuffer, null) ?? new ByteArray(Enforcement.TruncateMessageTo);
            var originalLogEntry = RenderLogEvent(Layout, logEvent);
            messageBuilder.PrepareMessage(buffer, logEvent, originalLogEntry);
            await messageTransmitter.SendMessageAsync(buffer, cancellationToken);
            Interlocked.Exchange(ref reusableBuffer, buffer);
        }

        /// <inheritdoc />
        protected override void CloseTarget()
        {
            Enforcement.PropertyChanged -= Init;
            MessageCreation.PropertyChanged -= Init;
            MessageSend.PropertyChanged -= Init;
            base.CloseTarget();
            messageTransmitter.Dispose();
        }
    }
}