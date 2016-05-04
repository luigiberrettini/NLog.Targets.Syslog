using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageSend
{
    public class MessageTransmittersFacade
    {
        private MessageTransmitter activeTransmitter;
        private readonly Dictionary<ProtocolType, MessageTransmitter> transmitters;

        /// <summary>The Syslog server protocol</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>UDP related fields</summary> 
        public UdpProtocol UdpProtocol { get; set; }

        /// <summary>TCP related fields</summary> 
        public TcpProtocol TcpProtocol { get; set; }

        /// <summary>Builds a new instance of the MessageTransmittersFacade class</summary>
        public MessageTransmittersFacade()
        {
            Protocol = ProtocolType.Tcp;
            UdpProtocol = new UdpProtocol();
            TcpProtocol = new TcpProtocol();
            transmitters = new Dictionary<ProtocolType, MessageTransmitter>
            {
                {ProtocolType.Udp, UdpProtocol},
                {ProtocolType.Tcp, TcpProtocol}
            };
        }

        internal void Initialize()
        {
            activeTransmitter = transmitters[Protocol];
        }

        internal IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return activeTransmitter.FrameMessageOrLeaveItUnchanged(syslogMessage);
        }

        internal void SendMessages(IEnumerable<byte[]> syslogMessages)
        {
            activeTransmitter.SendMessages(syslogMessages);
        }
    }
}