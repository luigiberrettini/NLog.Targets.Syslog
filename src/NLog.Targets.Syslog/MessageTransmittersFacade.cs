using System.Collections.Generic;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public class MessageTransmittersFacade : MessageTransmitter
    {
        private readonly Dictionary<ProtocolType, MessageTransmitter> transmitters;
        private MessageTransmitter ProtocolToUse => transmitters[Protocol];

        /// <summary>The Syslog server protocol</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>UDP related fields</summary> 
        public UdpProtocol UdpProtocol { get; set; }

        /// <summary>TCP related fields</summary> 
        public TcpProtocol TcpProtocol { get; set; }

        /// <summary>Initializes a new instance of the MessageTransmittersFacade class</summary>
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

        /// <summary>Applies the framing method of the protocol to use to a Syslog message</summary>
        /// <param name="syslogMessage">The message to be framed</param>
        /// <returns>Bytes containing the framed Syslog message or the original Syslog message</returns>
        public override IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return ProtocolToUse.FrameMessageOrLeaveItUnchanged(syslogMessage);
        }

        /// <summary>Sends a set of Syslog messages with the protocol to use</summary>
        /// <param name="syslogMessages">The messages to be sent</param>
        public override void SendMessages(IEnumerable<byte[]> syslogMessages)
        {
            ProtocolToUse.SendMessages(syslogMessages);
        }
    }
}