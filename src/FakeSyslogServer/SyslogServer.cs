// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Threading.Tasks;

namespace FakeSyslogServer
{
    public class SyslogServer
    {
        private readonly IPEndPoint udpEndPoint;
        private readonly IPEndPoint tcpEndPoint;
        private readonly UdpServer udpServer;
        private readonly TcpServer tcpServer;
        private volatile bool stopped;

        public static int UdpProtocolHashCode { get; }

        public static int TcpProtocolHashCode { get; }

        static SyslogServer()
        {
            UdpProtocolHashCode = "UDP".GetHashCode();
            TcpProtocolHashCode = "TCP".GetHashCode();
        }

        public SyslogServer(IPEndPoint udpEndPoint, IPEndPoint tcpEndPoint)
        {
            this.udpEndPoint = udpEndPoint;
            this.tcpEndPoint = tcpEndPoint;
            udpServer = new UdpServer();
            tcpServer = new TcpServer();
            stopped = true;
        }

        public void Start(Action<int, string> receivedStringAction, Action<Task> exceptionAction)
        {
            if (!stopped)
                return;

            stopped = false;

            Task.Factory
                .StartNew(() => udpServer.StartListening(udpEndPoint, str => receivedStringAction(UdpProtocolHashCode, str)))
                .ContinueWith(t =>
                 {
                     if (t.Exception != null)
                         exceptionAction(t);
                 });

            Task.Factory
                .StartNew(() => tcpServer.StartListening(tcpEndPoint, str => receivedStringAction(TcpProtocolHashCode, str)))
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        exceptionAction(t);
                });
        }

        public void Stop()
        {
            if (stopped)
                return;

            stopped = true;
            udpServer.StopListening();
            tcpServer.StopListening();
        }

        public void Dispose()
        {
            Stop();
            udpServer.Dispose();
            tcpServer.Dispose();
        }
    }
}