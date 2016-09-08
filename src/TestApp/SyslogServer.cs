using System;
using System.Net;
using System.Net.Sockets;

namespace TestApp
{
    public class SyslogServer : IDisposable
    {
        private readonly IPEndPoint udpEndPoint;
        private readonly IPEndPoint tcpEndPoint;
        private readonly UdpServer udpServer;
        private readonly TcpServer tcpServer;

        public SyslogServer(IPEndPoint udpEndPoint, IPEndPoint tcpEndPoint)
        {
            this.udpEndPoint = udpEndPoint;
            this.tcpEndPoint = tcpEndPoint;
            tcpServer = new TcpServer();
            udpServer = new UdpServer();
        }

        public void Start(Action<ProtocolType, string> receivedStringAction)
        {
            udpServer.StartListening(udpEndPoint, str => receivedStringAction(ProtocolType.Udp, str));
            tcpServer.StartListening(tcpEndPoint, str => receivedStringAction(ProtocolType.Tcp, str));
        }

        public void Dispose()
        {
            udpServer.Dispose();
            tcpServer.Dispose();
        }
    }
}