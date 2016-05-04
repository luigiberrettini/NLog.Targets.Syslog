using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;

namespace NLog.Targets.Syslog.MessageSend
{
    [DisplayName("Udp")]
    public class UdpProtocol : MessageTransmitter
    {
        /// <summary>Sends a set of Syslog messages with UDP and the related settings</summary>
        /// <param name="syslogMessages">The messages to be sent</param>
        public override void SendMessages(IEnumerable<byte[]> syslogMessages)
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