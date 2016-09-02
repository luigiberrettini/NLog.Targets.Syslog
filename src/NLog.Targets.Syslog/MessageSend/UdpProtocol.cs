using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Udp")]
    public class UdpProtocol : MessageTransmitter
    {
        private UdpClient udp;
        private volatile bool disposed;

        internal override void Initialize()
        {
            udp = new UdpClient(IpAddress, Port);
        }

        internal override Task SendMessageAsync(byte[] message, CancellationToken token)
        {
            return udp.SendAsync(message, message.Length);
        }

        internal override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            udp.Close();
        }
    }
}