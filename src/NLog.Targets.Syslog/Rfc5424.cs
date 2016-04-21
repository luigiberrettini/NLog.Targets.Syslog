using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Allows to build Syslog messages comliant with RFC 5424</summary>
    [NLogConfigurationItem]
    public class Rfc5424 : MessageBuilder
    {
        private const string NilValue = "-";
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private const int HostnameMaxLength = 255;
        private const int AppNameMaxLength = 48;
        private const int ProcIdMaxLength = 128;
        private const int MsgIdMaxLength = 32;
        private static readonly byte[] SpaceBytes = Encoding.ASCII.GetBytes(" ");
        private static readonly byte[] Bom = { 0xEF, 0xBB, 0xBF };

        /// <summary>The VERSION field of the HEADER part</summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname { get; set; }

        /// <summary>The APPNAME field of the HEADER part</summary>
        public Layout AppName { get; set; }

        /// <summary>The PROCID field of the HEADER part</summary>
        public Layout ProcId { get; set; }

        /// <summary>The MSGID field of the HEADER part</summary>
        public Layout MsgId { get; set; }

        /// <summary>The STRUCTURED-DATA part</summary>
        public StructuredData StructuredData { get; set; }

        /// <summary>Initializes a new instance of the Rfc5424 class</summary>
        public Rfc5424()
        {
            ProtocolVersion = 1;
            Hostname = Dns.GetHostName();
            AppName = Assembly.GetCallingAssembly().GetName().Name;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = new StructuredData();
        }

        /// <summary>Builds the Syslog message according to the RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Byte array containing the Syslog message</returns>
        public override IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            return HeaderBytes(pri, logEvent)
                .Concat(SpaceBytes)
                .Concat(StructuredData.Bytes(logEvent))
                .Concat(SpaceBytes)
                .Concat(MsgBytes(logEntry));
        }

        private IEnumerable<byte> HeaderBytes(string pri, LogEventInfo logEvent)
        {
            var version = ProtocolVersion.ToString(CultureInfo.InvariantCulture);
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = Hostname.RenderOrDefault(logEvent, HostnameMaxLength);
            var appName = AppName.RenderOrDefault(logEvent, AppNameMaxLength);
            var procId = ProcId.RenderOrDefault(logEvent, ProcIdMaxLength);
            var msgId = MsgId.RenderOrDefault(logEvent, MsgIdMaxLength);
            var header = $"{pri}{version} {timestamp} {hostname} {appName} {procId} {msgId}";
            return Encoding.ASCII.GetBytes(header);
        }

        private static IEnumerable<byte> MsgBytes(string logEntry)
        {
            return Bom.Concat(Encoding.UTF8.GetBytes(logEntry));
        }
    }
}