using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Udp")]
    public class UdpProtocol : MessageTransmitter
    {
        internal override void SendMessages(IEnumerable<byte[]> syslogMessages)
        {
            if (string.IsNullOrEmpty(IpAddress))
                return;

            using (var udp = new UdpClient(IpAddress, Port))
            {
                foreach (var message in syslogMessages)
                    udp.Send(message, message.Length);
            }
        }
    }
}