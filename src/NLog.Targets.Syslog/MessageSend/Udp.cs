// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Udp : MessageTransmitter
    {
        private readonly UdpClient udp;
        private volatile bool disposed;

        public Udp(UdpConfig udpConfig) : base(udpConfig.Server, udpConfig.Port)
        {
            udp = new UdpClient(IpAddress, Port);
        }

        public override Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            return udp.SendAsync(message, message.Length);
        }

        public override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            udp.Close();
        }
    }
}