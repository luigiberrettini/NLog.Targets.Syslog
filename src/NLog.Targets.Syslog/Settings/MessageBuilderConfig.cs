// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Message build configuration</summary>
    public class MessageBuilderConfig
    {
        /// <summary>The Syslog facility to log from (its name e.g. local0 or local7)</summary>
        public Facility Facility { get; set; }

        /// <summary>The Syslog protocol RFC to be followed</summary>
        public RfcNumber Rfc { get; set; }

        /// <summary>RFC 3164 related fields</summary>
        public Rfc3164Config Rfc3164 { get; set; }

        /// <summary>RFC 5424 related fields</summary>
        public Rfc5424Config Rfc5424 { get; set; }

        /// <summary>Builds a new instance of the MessageBuilderConfig class</summary>
        public MessageBuilderConfig()
        {
            Rfc = RfcNumber.Rfc5424;
            Rfc3164 = new Rfc3164Config();
            Rfc5424 = new Rfc5424Config();
        }
    }
}