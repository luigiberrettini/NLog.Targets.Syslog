using NLog.Config;
using NLog.Layouts;
using System;
using System.Globalization;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    [NLogConfigurationItem]
    public class Rfc3164 : MessageBuilder
    {
        private const string TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private Layout MachineName { get; }
        private Layout Sender { get; }

        /// <summary>Initializes a new instance of the Rfc3164 class</summary>
        public Rfc3164(Layout sender, Layout machineName)
        {
            Sender = sender;
            MachineName = machineName;
        }

        /// <summary>Builds the Syslog message according to RFC 3164</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Byte array containing the Syslog message</returns>
        protected override byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var header = Header(logEvent);
            var msg = Msg(logEvent, logEntry);

            var syslogMessage = $"{pri}{header} {msg}";

            return Encoding.ASCII.GetBytes(syslogMessage);
        }

        /// <summary>Syslog HEADER part</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <returns>String containing Syslog PRI part</returns>
        private string Header(LogEventInfo logEvent)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = MachineName.Render(logEvent);
            var header = $"{timestamp} {hostname}";
            return header;
        }

        /// <summary>Syslog MSG part</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>String containing Syslog MSG part</returns>
        private string Msg(LogEventInfo logEvent, string logEntry)
        {
            var tag = Sender.Render(logEvent);
            var content = Char.IsLetterOrDigit(logEntry[0]) ? " {logEntry}" : logEntry;
            var msg = $"{tag}{content}";
            return msg;
        }
    }
}