using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageCreation
{
    public class MessageBuildersFacade
    {
        private MessageBuilder activeBuilder;
        private readonly Dictionary<RfcNumber, MessageBuilder> builders;

        /// <summary>The Syslog facility to log from (its name e.g. local0 or local7)</summary>
        public Facility Facility { get; set; }

        /// <summary>The Syslog protocol RFC to be followed</summary> 
        public RfcNumber Rfc { get; set; }

        /// <summary>RFC 3164 related fields</summary> 
        public Rfc3164 Rfc3164 { get; set; }

        /// <summary>RFC 5424 related fields</summary> 
        public Rfc5424 Rfc5424 { get; set; }

        /// <summary>Builds a new instance of the MessageBuildersFacade class</summary>
        public MessageBuildersFacade()
        {
            Facility = Facility.Local1;
            Rfc = RfcNumber.Rfc5424;
            Rfc3164 = new Rfc3164();
            Rfc5424 = new Rfc5424();
            builders = new Dictionary<RfcNumber, MessageBuilder>
            {
                { RfcNumber.Rfc3164, Rfc3164 },
                { RfcNumber.Rfc5424, Rfc5424 }
            };
        }

        internal void Initialize(Enforcement enforcement)
        {
            activeBuilder = builders[Rfc];
            activeBuilder.Initialize(enforcement, Facility);
        }

        internal IEnumerable<IEnumerable<byte>> BuildMessages(LogEventInfo logEvent, Layout layout)
        {
            return activeBuilder.BuildMessages(logEvent, layout);
        }
    }
}