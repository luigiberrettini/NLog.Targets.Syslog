// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Tcp : MessageTransmitter
    {
        private static readonly byte[] LineFeedBytes = { 0x0A };
        private static readonly SocketInitialization SocketInitialization = SocketInitialization.ForCurrentOs();

        private readonly KeepAliveConfig keepAliveConfig;
        private readonly bool useTls;
        private readonly Func<X509Certificate2Collection> retrieveClientCertificates;
        private readonly FramingMethod framing;
        private TcpClient tcp;
        private Stream stream;

        public Tcp(TcpConfig tcpConfig) : base(tcpConfig.Server, tcpConfig.Port, tcpConfig.ReconnectInterval)
        {
            keepAliveConfig = tcpConfig.KeepAlive;
            useTls = tcpConfig.Tls.Enabled;
            retrieveClientCertificates = tcpConfig.Tls.RetrieveClientCertificates;
            framing = tcpConfig.Framing;
        }

        protected override Task Init()
        {
            tcp = new TcpClient();
            SocketInitialization.DisableAddressSharing(tcp.Client);
            SocketInitialization.DiscardPendingDataOnClose(tcp.Client);
            SocketInitialization.SetKeepAlive(tcp.Client, keepAliveConfig);

            return tcp
                .ConnectAsync(IpAddress, Port)
                .Then(_ => stream = SslDecorate(tcp), CancellationToken.None);
        }

        protected override Task SendAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            return HandleFramingAsync(message)
                .Then(_ => stream.WriteAsync(message, 0, message.Length, token), token)
                .Unwrap();
        }

        protected override void Terminate()
        {
            DisposeSslStreamNotTcpClientInnerStream();
            DisposeTcpClientAndItsInnerStream();
        }

        private Stream SslDecorate(TcpClient tcpClient)
        {
            var tcpStream = tcpClient.GetStream();

            if (!useTls)
                return tcpStream;

            // Do not dispose TcpClient inner stream when disposing SslStream (TcpClient disposes it)
            var sslStream = new SslStream(tcpStream, true);
            sslStream.AuthenticateAsClient(Server, retrieveClientCertificates(), SslProtocols.Tls12, false);

            return sslStream;
        }

        private Task HandleFramingAsync(ByteArray message)
        {
            if (framing == FramingMethod.NonTransparent)
            {
                message.AppendBytes(LineFeedBytes);
                return Task.FromResult<object>(null);
            }

            var octetCount = message.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");
            return stream.WriteAsync(prefix, 0, prefix.Length);
        }

        private void DisposeSslStreamNotTcpClientInnerStream()
        {
            if (useTls)
                stream?.Dispose();
        }

        private void DisposeTcpClientAndItsInnerStream()
        {
            tcp?.Close();
        }
    }
}