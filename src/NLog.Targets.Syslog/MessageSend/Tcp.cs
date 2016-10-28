using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Tcp : MessageTransmitter
    {
        private static readonly TimeSpan ZeroSecondsTimeSpan = TimeSpan.FromSeconds(0);
        private static readonly byte[] LineFeedBytes = { 0x0A };

        private volatile bool isFirstSend;
        private readonly TimeSpan recoveryTime;
        private readonly bool useTls;
        private readonly int dataChunkSize;
        private readonly FramingMethod framing;
        private readonly TcpClient tcp;
        private Stream stream;
        private volatile bool disposed;

        public Tcp(TcpConfig tcpConfig) : base(tcpConfig.Server, tcpConfig.Port)
        {
            isFirstSend = true;
            recoveryTime = TimeSpan.FromMilliseconds(tcpConfig.ReconnectInterval);
            useTls = tcpConfig.UseTls;
            framing = tcpConfig.Framing;
            dataChunkSize = tcpConfig.DataChunkSize;
            tcp = new TcpClient();
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        public override Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            if (tcp.Connected)
                return WriteAsync(message, token);

            var delay = isFirstSend ? ZeroSecondsTimeSpan : recoveryTime;
            isFirstSend = false;

            return Task.Delay(delay, token)
                .Then(_ => ConnectAsync(), token)
                .Unwrap()
                .Then(_ => WriteAsync(message, token), token)
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

            if (!useTls)
                return tcpStream;

            // Do not dispose TcpClient inner stream when disposing SslStream (TcpClient disposes it)
            var sslStream = new SslStream(tcpStream, true);
            sslStream.AuthenticateAsClient(Server, null, SslProtocols.Tls12, false);
            return sslStream;
        }

        private Task WriteAsync(ByteArray message, CancellationToken token)
        {
            return FramingTask(message)
                .Then(_ => WriteAsync(0, message, token), token)
                .Unwrap();
        }

        private Task FramingTask(ByteArray message)
        {
            if (framing == FramingMethod.NonTransparent)
            {
                message.Append(LineFeedBytes);
                return Task.FromResult<object>(null);
            }

            var octetCount = message.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, prefix, 0, prefix.Length, null);
        }

        private Task WriteAsync(int offset, ByteArray data, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            var toBeWrittenTotal = data.Length - offset;
            var isLastWrite = toBeWrittenTotal <= dataChunkSize;
            var count = isLastWrite ? toBeWrittenTotal : dataChunkSize;

            return Task.Factory
                .FromAsync(stream.BeginWrite, stream.EndWrite, (byte[])data, offset, count, null)
                .Then(task => isLastWrite ? task : WriteAsync(offset + dataChunkSize, data, token), token)
                .Unwrap();
        }

        public override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            // Dispose SslStream without disposing TcpClient inner stream
            if (useTls)
                stream.Dispose();

            // Dispose TcpClient and its inner stream
            tcp.Close();
        }
    }
}