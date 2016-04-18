using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Allows to build Syslog messages comliant with RFC 5424</summary>
    [NLogConfigurationItem]
    public class Rfc5424 : MessageBuilder
    {
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private readonly byte[] spaceBytes = Encoding.ASCII.GetBytes(" ");
        private const string NilValue = "-";
        private const int HostnameMaxLength = 255;
        private const int AppNameMaxLength = 48;
        private const int ProcIdMaxLength = 128;
        private const int MsgIdMaxLength = 32;
        private static readonly byte[] Bom = { 0xEF, 0xBB, 0xBF };
        private Layout Hostname { get; }
        private Layout Sender { get; }

        /// <summary>The VERSION field of the header</summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>The PROCID field of the header</summary>
        public Layout ProcId { get; set; }

        /// <summary>The MSGID field of the header</summary>
        public Layout MsgId { get; set; }

        /// <summary>The STRUCTURED-DATA part of the message</summary>
        [ArrayParameter(typeof(SdElement), nameof(SdElement))]
        public IList<SdElement> StructuredData { get; set; }

        /// <summary>Initializes a new instance of the Rfc5424 class</summary>
        public Rfc5424(Layout sender, Layout hostname)
        {
            Sender = sender;
            Hostname = hostname;
            ProtocolVersion = 1;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = new List<SdElement>();
        }

        /// <summary>Builds the Syslog message according to the RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Byte array containing the Syslog message</returns>
        public override byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var message = new List<byte>();
            message.AddRange(HeaderBytes(pri, logEvent));
            message.AddRange(spaceBytes);
            message.AddRange(StructuredDataBytes(logEvent));
            message.AddRange(spaceBytes);
            message.AddRange(MsgBytes(logEntry));
            return message.ToArray();
        }

        private IEnumerable<byte> HeaderBytes(string pri, LogEventInfo logEvent)
        {
            var version = ProtocolVersion.ToString(CultureInfo.InvariantCulture);
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = Hostname.Render(logEvent, HostnameMaxLength, NilValue);
            var appName = Sender.Render(logEvent, AppNameMaxLength, NilValue);
            var procId = ProcId.Render(logEvent, ProcIdMaxLength, NilValue);
            var msgId = MsgId.Render(logEvent, MsgIdMaxLength, NilValue);
            var header = $"{pri}{version} {timestamp} {hostname} {appName} {procId} {msgId}";
            return Encoding.ASCII.GetBytes(header);
        }

        private IEnumerable<byte> StructuredDataBytes(LogEventInfo logEvent)
        {
            return StructuredData.Count == 0 ? Encoding.ASCII.GetBytes(NilValue) : StructuredData.SelectMany(sdElement => sdElement.Bytes(logEvent));
        }

        private static IEnumerable<byte> MsgBytes(string logEntry)
        {
            var msgBytes = new List<byte>();
            msgBytes.AddRange(Bom);
            msgBytes.AddRange(Encoding.UTF8.GetBytes(logEntry));
            return msgBytes;
        }
    }
}