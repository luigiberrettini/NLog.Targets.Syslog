// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal abstract class MessageBuilder
    {
        private static readonly Dictionary<RfcNumber, Func<MessageBuilderConfig, EnforcementConfig, MessageBuilder>> BuilderFactory;
        private static readonly Dictionary<Facility, Dictionary<Severity, string>> PriForFacilityAndSeverity;

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

            PriForFacilityAndSeverity = Enum
                .GetValues(typeof(Facility))
                .Cast<Facility>()
                .ToDictionary
                (
                    facility => facility,
                    facility => Enum
                        .GetValues(typeof(Severity))
                        .Cast<Severity>()
                        .ToDictionary(severity => severity, severity => Pri(facility, severity))
                );
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

        public string[] BuildLogEntries(LogEventInfo logEvent, Layout layout)
        {
            if (logEvent.Level == LogLevel.Off)
                return new string[0];
            var originalLogEntry = layout.Render(logEvent);
            return splitOnNewLinePolicy.IsApplicable() ? splitOnNewLinePolicy.Apply(originalLogEntry) : new[] { originalLogEntry };
        }

        public void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string logEntry)
        {
            buffer.Reset();
            var severity = logLevelSeverityMapping[logEvent.Level];
            var pri = PriForFacilityAndSeverity[facility][severity];
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