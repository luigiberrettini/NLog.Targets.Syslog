using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    internal class SyslogLogEventInfo
    {
        private static readonly char[] LineSeps = { '\r', '\n' };

        public LogEventInfo LogEvent { get; }

        public string Pri { get; private set; }

        public List<string> LogEntries { get; private set; }

        /// <summary>Initializes a new instance of the SyslogLogEventInfo class</summary>
        public SyslogLogEventInfo(LogEventInfo logEvent)
        {
            LogEvent = logEvent;
        }

        public SyslogLogEventInfo Build(SyslogFacility facility, Layout layout, bool splitNewlines)
        {
            Pri = Priority(facility, (SyslogSeverity)LogEvent.Level);
            LogEntries = RenderedLogEntries(layout, splitNewlines).ToList();
            return this;
        }

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
        /// <param name="layout">The NLog.LogEventInfo</param>
        /// <param name="splitNewlines">Determines if the original log entry is to be split by newlines</param>
        private IEnumerable<string> RenderedLogEntries(Layout layout, bool splitNewlines)
        {
            var originalLogEntry = layout.Render(LogEvent);
            return splitNewlines ? originalLogEntry.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries) : new[] { originalLogEntry };
        }
    }
}