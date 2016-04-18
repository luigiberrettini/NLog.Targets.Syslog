// ReSharper disable CheckNamespace


using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Allows to build Syslog messages</summary>
    public abstract class MessageBuilder
    {
        private static readonly char[] LineSeps = { '\r', '\n' };

        /// <summary>Convert a message builder to an RFC number</summary>
        /// <param name="messageBuilder">Syslog messageBuilder to convert</param>
        /// <returns>The RFC number which corresponds to the message builder</returns>
        public static explicit operator RfcNumber(MessageBuilder messageBuilder)
        {
            return (RfcNumber)Enum.Parse(typeof(RfcNumber), messageBuilder.GetType().Name);
        }

        public IEnumerable<byte[]> BuildMessages(SyslogFacility facility, LogEventInfo logEvent, Layout layout, bool splitNewlines)
        {
            var pri = Priority(facility, (SyslogSeverity)logEvent.Level);
            var logEntries = RenderedLogEntries(logEvent, layout, splitNewlines).ToList();
            var toBeSent = logEntries.Select(logEntry => BuildMessage(logEvent, pri, logEntry));
            return toBeSent;
        }

        protected abstract byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry);

        /// <summary>Determines the Syslog PRI part</summary>
        /// <param name="facility">Syslog facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <returns>The string representing Syslog PRI part</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Priority(SyslogFacility facility, SyslogSeverity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            var priValString = priVal.ToString(CultureInfo.InvariantCulture);
            return $"<{priValString}>";
        }

        /// <summary>Renders log entries</summary>
        /// <param name="logEvent"></param>
        /// <param name="layout">The NLog.LogEventInfo</param>
        /// <param name="splitNewlines">Determines if the original log entry is to be split by newlines</param>
        private static IEnumerable<string> RenderedLogEntries(LogEventInfo logEvent, Layout layout, bool splitNewlines)
        {
            var originalLogEntry = layout.Render(logEvent);
            return splitNewlines ? originalLogEntry.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries) : new[] { originalLogEntry };
        }
    }
}