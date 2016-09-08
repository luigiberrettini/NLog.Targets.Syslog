using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            udpServer = new UdpServer();
            tcpServer = new TcpServer();
        }

        public void Start(Action<ProtocolType, string> receivedStringAction, Action<Task> exceptionAction)
        {
            Task.Factory
                .StartNew(() => udpServer.StartListening(udpEndPoint, str => receivedStringAction(ProtocolType.Udp, str)))
                .ContinueWith(t =>
                 {
                     if (t.Exception != null)
                         exceptionAction(t);
                 });

            Task.Factory
                .StartNew(() => tcpServer.StartListening(tcpEndPoint, str => receivedStringAction(ProtocolType.Tcp, str)))
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        exceptionAction(t);
                });
        }

        public void Dispose()
        {
            udpServer.Dispose();
            tcpServer.Dispose();
        }
    }
}