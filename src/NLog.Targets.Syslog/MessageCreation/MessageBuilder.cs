using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Globalization;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages</summary>
    public abstract class MessageBuilder
    {
        private SplitOnNewLinePolicy splitOnNewLinePolicy;

        /// <summary>The buffer used to build a Syslog message</summary>
        protected ByteArray Message { get; set; }

        internal virtual void Initialize(Enforcement enforcement)
        {
            splitOnNewLinePolicy = new SplitOnNewLinePolicy(enforcement);
            Message = new ByteArray(enforcement.TruncateMessageTo);
        }

        internal IEnumerable<string> BuildLogEntries(LogEventInfo logEvent, Layout layout)
        {
            var originalLogEntry = layout.Render(logEvent);
            return splitOnNewLinePolicy.IsApplicable() ? splitOnNewLinePolicy.Apply(originalLogEntry) : new[] { originalLogEntry };
        }

        internal ByteArray BuildMessage(Facility facility, LogEventInfo logEvent, string logEntry)
        {
            Message.Reset();
            var pri = Pri(facility, (Severity)logEvent.Level);
            return BuildMessage(logEvent, pri, logEntry);
        }

        protected abstract ByteArray BuildMessage(LogEventInfo logEvent, string pri, string logEntry);

        private static string Pri(Facility facility, Severity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            var priValString = priVal.ToString(CultureInfo.InvariantCulture);
            return $"<{priValString}>";
        }

        internal void Dispose()
        {
            Message.Dispose();
        }
    }
}