using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Tcp")]
    public class TcpProtocol : MessageTransmitter
    {
        private FramingMethod framing;
        private static readonly byte[] LineFeedBytes = { 0x0A };

        /// <summary>Whether to use TLS or not (TLS 1.2 only)</summary>
        public bool UseTls { get; set; }

        /// <summary>Which framing method to use</summary>
        /// <remarks>If <see cref="UseTls">is true</see> get will always return OctetCounting (RFC 5425)</remarks>
        public FramingMethod Framing
        {
            get { return UseTls ? FramingMethod.OctetCounting : framing; }
            set { framing = value; }
        }

        /// <summary>Builds a new instance of the TcpProtocol class</summary>
        public TcpProtocol()
        {
            UseTls = true;
            Framing = FramingMethod.OctetCounting;
        }

        internal override IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return OctectCountingFramedOrUnchanged(NonTransparentFramedOrUnchanged(syslogMessage));
        }

        internal override void SendMessages(IEnumerable<byte[]> syslogMessages)
        {
            if (string.IsNullOrEmpty(IpAddress))
                return;

            using (var tcp = new TcpClient(IpAddress, Port))
            using (var stream = SslDecorate(tcp))
            {
                foreach (var message in syslogMessages)
                    stream.Write(message, 0, message.Length);
            }
        }

        private IEnumerable<byte> OctectCountingFramedOrUnchanged(IEnumerable<byte> syslogMessage)
        {
            if (Framing == FramingMethod.OctetCounting)
                return syslogMessage;

            var slMessage = syslogMessage.ToArray();
            var octetCount = slMessage.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");
            return prefix.Concat(slMessage);
        }

        private IEnumerable<byte> NonTransparentFramedOrUnchanged(IEnumerable<byte> syslogMessage)
        {
            return Framing == FramingMethod.NonTransparent ? syslogMessage.Concat(LineFeedBytes) : syslogMessage;
        }

        private Stream SslDecorate(TcpClient tcp)
        {
            var tcpStream = tcp.GetStream();

            if (!UseTls)
                return tcpStream;

            var sslStream = new SslStream(tcpStream, true);
            sslStream.AuthenticateAsClient(Server, null, SslProtocols.Tls12, false);
            return sslStream;
        }
    }
}