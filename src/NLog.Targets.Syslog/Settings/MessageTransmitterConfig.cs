// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Message transmission configuration</summary>
    public class MessageTransmitterConfig
    {
        /// <summary>The Syslog server protocol</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>UDP related fields</summary>
        public UdpConfig Udp { get; set; }

        /// <summary>TCP related fields</summary>
        public TcpConfig Tcp { get; set; }

        /// <summary>Builds a new instance of the MessageTransmitterConfig class</summary>
        public MessageTransmitterConfig()
        {
            Udp = new UdpConfig();
            Tcp = new TcpConfig();
        }
    }
}