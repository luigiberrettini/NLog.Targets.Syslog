// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;
using NLog.Targets.Syslog.Policies;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal abstract class MessageBuilder
    {
        private static readonly Dictionary<RfcNumber, Func<MessageBuilderConfig, EnforcementConfig, MessageBuilder>> BuilderFactory;
        private readonly SplitOnNewLinePolicy splitOnNewLinePolicy;
        private readonly Facility facility;
        private readonly LogLevelSeverityMapping logLevelSeverityMapping;

        static MessageBuilder()
        {
            BuilderFactory = new Dictionary<RfcNumber, Func<MessageBuilderConfig, EnforcementConfig, MessageBuilder>>
            {
                { RfcNumber.Rfc3164, (msgBuilderCfg, enforcementCfg) =>
                    new Rfc3164(msgBuilderCfg.Facility, msgBuilderCfg.PerLogLevelSeverity, msgBuilderCfg.Rfc3164, enforcementCfg) },
                { RfcNumber.Rfc5424, (msgBuilderCfg, enforcementCfg) =>
                    new Rfc5424(msgBuilderCfg.Facility, msgBuilderCfg.PerLogLevelSeverity, msgBuilderCfg.Rfc5424, enforcementCfg) }
            };
        }

        public static MessageBuilder FromConfig(MessageBuilderConfig messageBuilderConfig, EnforcementConfig enforcementConfig)
        {
            return BuilderFactory[messageBuilderConfig.Rfc](messageBuilderConfig, enforcementConfig);
        }

        protected MessageBuilder(Facility facility, LogLevelSeverityConfig logLevelSeverityConfig, EnforcementConfig enforcementConfig)
        {
            this.facility = facility;
            logLevelSeverityMapping = new LogLevelSeverityMapping(logLevelSeverityConfig);
            splitOnNewLinePolicy = new SplitOnNewLinePolicy(enforcementConfig);
        }

        public string BuildLogEntry(LogEventInfo logEvent, Layout layout, out string[] splitLogEntries)
        {
            splitLogEntries = null;

            if (logEvent.Level == LogLevel.Off)
            {
                return null;
            }

            var originalLogEntry = layout.Render(logEvent);
            if (splitOnNewLinePolicy.IsApplicable(originalLogEntry))
            {
                var logEntries = splitOnNewLinePolicy.Apply(originalLogEntry);
                if (logEntries.Length == 0)
                    return string.Empty;
                if (logEntries.Length == 1)
                    return logEntries[0];

                splitLogEntries = logEntries;
                return splitLogEntries[0];
            }

            return originalLogEntry;
        }

        public void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string logEntry)
        {
            buffer.Reset();
            var pri = Pri(facility, logLevelSeverityMapping[logEvent.Level]);
            PrepareMessage(buffer, logEvent, pri, logEntry);
        }

        protected abstract void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry);

        private static string Pri(Facility facility, Severity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            return $"<{priVal.ToString(CultureInfo.InvariantCulture)}>";
        }
    }
}