using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages</summary>
    public abstract class MessageBuilder
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private SplitOnNewLinePolicy splitOnNewLinePolicy;

        /// <summary>The buffer used to build a Syslog message</summary>
        protected MemoryStream messageBuffer { get; set; }

        /// <summary>The Syslog facility to log from (its name e.g. local0 or local7)</summary>
        protected Facility Facility { get; set; }

        internal virtual void Initialize(Enforcement enforcement, Facility facility)
        {
            splitOnNewLinePolicy = new SplitOnNewLinePolicy(enforcement);
            var capacity = enforcement.TruncateMessageTo != 0 ? enforcement.TruncateMessageTo : DefaultBufferCapacity;
            messageBuffer = new MemoryStream(capacity);
            Facility = facility;
        }

        internal IEnumerable<string> BuildLogEntries(LogEventInfo logEvent, Layout layout)
        {
            var originalLogEntry = layout.Render(logEvent);
            return splitOnNewLinePolicy.IsApplicable() ? splitOnNewLinePolicy.Apply(originalLogEntry) : new[] { originalLogEntry };
        }

        internal IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string logEntry)
        {
            var pri = Pri(Facility, (Severity)logEvent.Level);
            messageBuffer.SetLength(Zero);
            var toBeSent = BuildMessage(logEvent, pri, logEntry);
            return toBeSent;
        }

        internal abstract IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry);

        private static string Pri(Facility facility, Severity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            var priValString = priVal.ToString(CultureInfo.InvariantCulture);
            return $"<{priValString}>";
        }

        internal void Dispose()
        {
            messageBuffer.SetLength(Zero);
            messageBuffer.Capacity = Zero;
            messageBuffer.Dispose();
        }
    }
}