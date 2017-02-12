// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using NLog.Common;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        private volatile bool inited;
        private MessageBuilder messageBuilder;
        private AsyncLogger[] asyncLoggers;

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

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (inited)
                DisposeDependencies();

            messageBuilder = MessageBuilder.FromConfig(MessageCreation, Enforcement);
            asyncLoggers = Enforcement.MessageProcessors.Select(i => new AsyncLogger(Layout, Enforcement, messageBuilder, MessageSend)).ToArray();
            inited = true;
        }

        /// <summary>Writes a single event</summary>
        /// <param name="asyncLogEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to override it</remarks>
        protected override void Write(AsyncLogEventInfo asyncLogEvent)
        {
            var logEvent = asyncLogEvent.LogEvent;
            MergeEventProperties(logEvent);
            PrecalculateVolatileLayouts(logEvent);
            var asyncLoggerId = logEvent.SequenceID % Enforcement.MessageProcessors;
            asyncLoggers[asyncLoggerId].Log(asyncLogEvent);
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
                Enforcement.MessageProcessors.ForEach(i => asyncLoggers[i].Dispose());
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, $"{GetType().Name} dispose error");
            }
        }
    }
}