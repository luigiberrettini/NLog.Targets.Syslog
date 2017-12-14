// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Settings;

namespace FakeSyslogServer
{
    public class SyslogServer
    {
        private readonly IPEndPoint udpEndPoint;
        private readonly IPEndPoint tcpEndPoint;
        private readonly UdpServer udpServer;
        private readonly TcpServer tcpServer;
        private volatile bool stopped;

        public SyslogServer(IPEndPoint udpEndPoint, IPEndPoint tcpEndPoint)
        {
            this.udpEndPoint = udpEndPoint;
            this.tcpEndPoint = tcpEndPoint;
            udpServer = new UdpServer();
            tcpServer = new TcpServer();
            stopped = true;
        }

        public void Start(Action<ProtocolType, string> receivedStringAction, Action<Task> exceptionAction)
        {
            if (!stopped)
                return;

            stopped = false;

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