using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    [NLogConfigurationItem]
    public class Rfc5424 : MessageBuilder
    {
        private const string NilValue = "-";
        private const int MachineNameMaxLength = 255;
        private const int SenderMaxLength = 48;
        private const int ProcIdMaxLength = 128;
        private const int MsgIdMaxLength = 32;
        private static readonly byte[] Bom = { 0xEF, 0xBB, 0xBF };
        private Layout MachineName { get; }
        private Layout Sender { get; }

        /// <summary>The protocol version</summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>Layout for PROCID protocol field</summary>
        public Layout ProcId { get; set; }

        /// <summary>Layout for MSGID protocol field</summary>
        public Layout MsgId { get; set; }

        /// <summary>Layout for STRUCTURED-DATA protocol field</summary>
        public Layout StructuredData { get; set; }

        public Rfc5424(Layout sender, Layout machineName)
        {
            Sender = sender;
            MachineName = machineName;
            ProtocolVersion = 1;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = NilValue;
        }

        /// <summary>Builds the Syslog message according to the RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Byte array containing the Syslog message</returns>
        protected override byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var version = ProtocolVersion.ToString(CultureInfo.InvariantCulture);
            var time = logEvent.TimeStamp.ToString("o");
            var machineName = MachineName.Render(logEvent, MachineNameMaxLength);
            var sender = Sender.Render(logEvent, SenderMaxLength);
            var procId = ProcId.Render(logEvent, ProcIdMaxLength);
            var msgId = MsgId.Render(logEvent, MsgIdMaxLength);

            var headerData = Encoding.ASCII.GetBytes($"{pri}{version} {time} {machineName} {sender} {procId} {msgId} ");
            var structuredData = Encoding.UTF8.GetBytes($"{StructuredData.Render(logEvent)} ");
            var messageData = Encoding.UTF8.GetBytes(logEntry);

            var allData = new List<byte>(headerData.Length + structuredData.Length + Bom.Length + messageData.Length);
            allData.AddRange(headerData);
            allData.AddRange(structuredData);
            allData.AddRange(Bom);
            allData.AddRange(messageData);

            return allData.ToArray();
        }
    }
}