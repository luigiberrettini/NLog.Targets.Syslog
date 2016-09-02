using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Tcp")]
    public class TcpProtocol : MessageTransmitter
    {
        private const int DefaultReconnectInterval = 500;
        private FramingMethod framing;
        private static readonly byte[] LineFeedBytes = { 0x0A };
        private volatile bool isFirstSend;
        private TimeSpan recoveryTime;
        private TcpClient tcp;
        private Stream stream;
        private volatile bool disposed;

        /// <summary>The time interval, in milliseconds, after which a connection is retried</summary>
        public int ReconnectInterval
        {
            get { return recoveryTime.Milliseconds; }
            set { recoveryTime = TimeSpan.FromMilliseconds(value); }
        }

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
            isFirstSend = true;
            ReconnectInterval = DefaultReconnectInterval;
            UseTls = true;
            Framing = FramingMethod.OctetCounting;
        }

        internal override void Initialize()
        {
            tcp = new TcpClient();
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        internal override IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> message)
        {
            return OctectCountingFramedOrUnchanged(NonTransparentFramedOrUnchanged(message));
        }

        internal override Task SendMessageAsync(byte[] message, CancellationToken token)
        {
            if (tcp.Connected)
                return WriteAsync(0, message, token);

            var delay = isFirstSend ? TimeSpan.FromSeconds(0) : recoveryTime;
            isFirstSend = false;

            return Task.Delay(delay, token)
                .Then(_ => ConnectAsync(), token)
                .Unwrap()
                .Then(_ => WriteAsync(0, message, token), token)
                .Unwrap();
        }

        private Task ConnectAsync()
        {
            return tcp
                .ConnectAsync(IpAddress, Port)
                .Then(_ => stream = SslDecorate(tcp), CancellationToken.None);
        }

        private Stream SslDecorate(TcpClient tcpClient)
        {
            var tcpStream = tcpClient.GetStream();

            if (!UseTls)
                return tcpStream;

            // Do not dispose TcpClient inner stream when disposing SslStream (TcpClient disposes it)
            var sslStream = new SslStream(tcpStream, true);
            sslStream.AuthenticateAsClient(Server, null, SslProtocols.Tls12, false);
            return sslStream;
        }

        private IEnumerable<byte> OctectCountingFramedOrUnchanged(IEnumerable<byte> message)
        {
            if (Framing != FramingMethod.OctetCounting)
                return message;

            var messageAsArray = message.ToArray();
            var octetCount = messageAsArray.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");
            return prefix.Concat(messageAsArray);
        }

        private IEnumerable<byte> NonTransparentFramedOrUnchanged(IEnumerable<byte> message)
        {
            return Framing != FramingMethod.NonTransparent ? message : message.Concat(LineFeedBytes);
        }

        private Task WriteAsync(int offset, byte[] data, CancellationToken token)
        {
            var toBeWritten = data.Length - offset;
            var isLastWrite = toBeWritten <= BufferSize;
            var size = isLastWrite ? toBeWritten : BufferSize;
            var buffer = new Byte[size];
            Buffer.BlockCopy(data, offset, buffer, 0, buffer.Length);

            return Task.Factory
                .FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null)
                .Then(task => isLastWrite ? task : WriteAsync(offset + BufferSize, data, token), token)
                .Unwrap();
        }

        internal override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            // Dispose SslStream without disposing TcpClient inner stream
            if (UseTls)
                stream.Dispose();

            // Dispose TcpClient and its inner stream
            tcp.Close();
        }
    }
}