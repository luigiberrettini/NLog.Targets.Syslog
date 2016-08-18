using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.MessageSend
{
    public class MessageTransmittersFacade
    {
        private const int DefaultRetryInterval = 5;
        private TimeSpan retryTime;
        private MessageTransmitter activeTransmitter;
        private readonly Dictionary<ProtocolType, MessageTransmitter> transmitters;

        /// <summary>The time interval, in seconds, after which a send is retried</summary>
        public int RetryInterval
        {
            get { return retryTime.Seconds; }
            set { retryTime = TimeSpan.FromSeconds(value); }
        }

        /// <summary>The Syslog server protocol</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>UDP related fields</summary>
        public UdpProtocol UdpProtocol { get; set; }

        /// <summary>TCP related fields</summary>
        public TcpProtocol TcpProtocol { get; set; }

        /// <summary>Builds a new instance of the MessageTransmittersFacade class</summary>
        public MessageTransmittersFacade()
        {
            RetryInterval = DefaultRetryInterval;
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
            activeTransmitter.Initialize();
        }

        internal Task SendMessageAsync(IEnumerable<byte> message, CancellationToken token)
        {
            return activeTransmitter.SendMessageAsync(FrameMessageOrLeaveItUnchanged(message), token);
        }

        private byte[] FrameMessageOrLeaveItUnchanged(IEnumerable<byte> message)
        {
            return activeTransmitter.FrameMessageOrLeaveItUnchanged(message).ToArray();
        }

        internal void Dispose()
        {
            activeTransmitter.Dispose();
        }
    }
}