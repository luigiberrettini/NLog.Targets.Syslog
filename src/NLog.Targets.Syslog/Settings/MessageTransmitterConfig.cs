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

        public MessageTransmitterConfig()
        {
            Udp = new UdpConfig();
            Tcp = new TcpConfig();
        }
    }
}