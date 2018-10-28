// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Udp : MessageTransmitter
    {
        private readonly int connectionCheckTimeout;
        private UdpClient udp;

        protected override bool Ready => udp?.Client?.IsConnected(connectionCheckTimeout) == true;

        public Udp(UdpConfig udpConfig) : base(udpConfig.Server, udpConfig.Port, udpConfig.ReconnectInterval)
        {
            connectionCheckTimeout = udpConfig.ConnectionCheckTimeout;
        }

        protected override Task Init()
        {
            udp = new UdpClient(IpAddress, Port);
            return Task.FromResult<object>(null);
        }

        protected override Task SendAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);
            return udp.SendAsync(message, message.Length);
        }

        protected override void Terminate()
        {
            udp?.Close();
        }
    }
}