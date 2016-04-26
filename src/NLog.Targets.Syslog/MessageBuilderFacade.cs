using NLog.Layouts;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public class MessageBuilderFacade : MessageBuilder
    {
        private readonly MessageBuilder[] messageBuilders;

        private MessageBuilder RfcToFollow { get; set; }

        /// <summary>The Syslog protocol RFC to be followed</summary> 
        public RfcNumber Rfc { get; set; }

        /// <summary>RFC 3164 related fields</summary> 
        public Rfc3164 Rfc3164 { get; set; }

        /// <summary>RFC 5424 related fields</summary> 
        public Rfc5424 Rfc5424 { get; set; }

        public MessageBuilderFacade()
        {
            Rfc = RfcNumber.Rfc5424;
            Rfc3164 = new Rfc3164();
            Rfc5424 = new Rfc5424();
            messageBuilders = new MessageBuilder[] { Rfc3164, Rfc5424 };
        }

        public new IEnumerable<byte[]> BuildMessages(SyslogFacility facility, LogEventInfo logEvent, Layout layout, bool splitNewlines)
        {
            RfcToFollow = messageBuilders.Single(x => (RfcNumber)x == Rfc);
            return RfcToFollow.BuildMessages(facility, logEvent, layout, splitNewlines);
        }

        public override IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            return RfcToFollow.BuildMessage(logEvent, pri, logEntry);
        }
    }
}