// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.Settings;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
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
            Enforcement.PropertyChanged += OnConfigurationChanges;
            MessageCreation.PropertyChanged += OnConfigurationChanges;
            MessageSend.PropertyChanged += OnConfigurationChanges;
        }

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            Init();
        }

        /// <summary>Writes a single event</summary>
        /// <param name="asyncLogEventInfo">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to override it</remarks>
        protected override void Write(AsyncLogEventInfo asyncLogEventInfo)
        {
            var logEvent = asyncLogEventInfo.LogEvent;
            PrecalculateVolatileLayouts(logEvent);
            var asyncLoggerId = logEvent.SequenceID % Enforcement.MessageProcessors;
            asyncLoggers[asyncLoggerId].Log(asyncLogEventInfo);
        }

        /// <summary>Flushes any pending log message</summary>
        /// <param name="asyncContinuation">The asynchronous continuation</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            InternalLogger.Debug("[Syslog] Explicit flush started");
            var tasks = Enforcement.MessageProcessors.Select(i => asyncLoggers[i].FlushAsync()).ToArray();
            Task.WhenAll(tasks)
                .ContinueWith(t =>
                {
                    var exception = t.Exception?.GetBaseException();
                    InternalLogger.Debug(exception, "[Syslog] Explicit flush completed");
                    asyncContinuation(exception);
                });
        }

        /// <summary>Closes the target</summary>
        protected override void CloseTarget()
        {
            DisposeDependencies();
            base.CloseTarget();
        }

        /// <summary>Disposes the instance</summary>
        protected override void Dispose(bool disposing)
        {
            Enforcement.PropertyChanged -= OnConfigurationChanges;
            MessageCreation.PropertyChanged -= OnConfigurationChanges;
            MessageSend.PropertyChanged -= OnConfigurationChanges;
            DisposeDependencies();
            base.Dispose(disposing);
        }

        private void OnConfigurationChanges(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (!IsInitialized)
                return;
            DisposeDependencies();
            Init();
        }

        private void Init()
        {
            messageBuilder = MessageBuilder.FromConfig(MessageCreation, Enforcement);
            asyncLoggers = Enforcement.MessageProcessors.Select(i => new AsyncLogger(Layout, Enforcement, messageBuilder, MessageSend)).ToArray();
        }

        private void DisposeDependencies()
        {
            try
            {
                Enforcement.MessageProcessors.ForEach(i => asyncLoggers[i]?.Dispose());
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "[Syslog] Dispose error");
            }
            finally
            {
                asyncLoggers = null;
            }
        }
    }
}