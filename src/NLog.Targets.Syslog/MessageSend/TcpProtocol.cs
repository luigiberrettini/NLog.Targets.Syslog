using System;
using System.ComponentModel;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Tcp")]
    public class TcpProtocol : MessageTransmitter
    {
        private const int DefaultReconnectInterval = 500;
        private const int DefaultBufferSize = 4096;
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

        /// <summary>The size of chunks in which data is split to be sent over the wire</summary>
        public int DataChunkSize { get; set; }

        /// <summary>Builds a new instance of the TcpProtocol class</summary>
        public TcpProtocol()
        {
            isFirstSend = true;
            ReconnectInterval = DefaultReconnectInterval;
            UseTls = true;
            Framing = FramingMethod.OctetCounting;
            DataChunkSize = DefaultBufferSize;
        }

        internal override void Initialize()
        {
            tcp = new TcpClient();
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        internal override Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            FrameMessageOrLeaveItUnchanged(message);

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

        private void FrameMessageOrLeaveItUnchanged(ByteArray message)
        {
            OctectCountingFramedOrUnchanged(message);
            NonTransparentFramedOrUnchanged(message);
        }

        private void OctectCountingFramedOrUnchanged(ByteArray message)
        {
            if (Framing != FramingMethod.OctetCounting)
                return;

            var octetCount = message.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");
            message.Prepend(prefix);
        }

        private void NonTransparentFramedOrUnchanged(ByteArray message)
        {
            if (Framing == FramingMethod.NonTransparent)
                message.Append(LineFeedBytes);
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

        private Task WriteAsync(int offset, ByteArray data, CancellationToken token)
        {
            var toBeWrittenTotal = data.Length - offset;
            var isLastWrite = toBeWrittenTotal <= DataChunkSize;
            var count = isLastWrite ? toBeWrittenTotal : DataChunkSize;

            return Task.Factory
                .FromAsync(stream.BeginWrite, stream.EndWrite, (byte[])data, offset, count, null)
                .Then(task => isLastWrite ? task : WriteAsync(offset + DataChunkSize, data, token), token)
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