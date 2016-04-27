using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public class TcpProtocol : MessageTransmitter
    {
        private static readonly byte[] LineFeedBytes = { 0x0A };

        /// <summary>Whether to use TLS or not (TLS 1.2 only)</summary>
        public bool UseTls { get; set; }

        /// <summary>Which framing method to use</summary>
        public FramingMethod Framing { get; set; }

        /// <summary>Initializes a new instance of the TcpProtocol class</summary>
        public TcpProtocol()
        {
            UseTls = true;
            Framing = FramingMethod.OctetCounting;
        }

        /// <summary>Applies a framing method to a Syslog message</summary>
        /// <param name="syslogMessage">The message to be framed</param>
        /// <returns>Bytes containing the framed Syslog message</returns>
        public override IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return OctectCountingFramedOrUnchanged(NonTransparentFramedOrUnchanged(syslogMessage));
        }

        /// <summary>Sends a set of Syslog messages with TCP and the related settings</summary>
        /// <param name="syslogMessages">The messages to be sent</param>
        public override void SendMessages(IEnumerable<byte[]> syslogMessages)
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

        private IEnumerable<byte> OctectCountingFramedOrUnchanged(IEnumerable<byte> source)
        {
            if (Framing == FramingMethod.OctetCounting)
                return source;

            var src = source.ToArray();
            var octetCount = src.Length;
            var prefix = Encoding.ASCII.GetBytes($"{octetCount} ");
            return prefix.Concat(src);
        }

        private IEnumerable<byte> NonTransparentFramedOrUnchanged(IEnumerable<byte> source)
        {
            return Framing == FramingMethod.NonTransparent ? source.Concat(LineFeedBytes) : source;
        }

        private Stream SslDecorate(TcpClient tcp)
        {
            var tcpStream = tcp.GetStream();

            if (!UseTls)
                return tcpStream;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var sslStream = new SslStream(tcpStream, true);
            sslStream.AuthenticateAsClient(Server);
            return sslStream;
        }
    }
}