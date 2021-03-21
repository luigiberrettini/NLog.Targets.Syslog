// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Udp : MessageTransmitter
    {
        private UdpClient udp;

        public Udp(UdpConfig udpConfig) : base(udpConfig.Server?.Render(LogEventInfo.CreateNullEvent()), udpConfig.Port, udpConfig.ReconnectInterval)
        {
        }

        protected override Task Init()
        {
            var ipEndPoint = GetIpEndPoint();
            udp = new UdpClient(ipEndPoint.AddressFamily);
            udp.Connect(ipEndPoint);
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