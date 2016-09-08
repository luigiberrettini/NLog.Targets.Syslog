using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    internal class UdpServer: ServerSocket
    {
        private static readonly ManualResetEvent Signal = new ManualResetEvent(false);
        private readonly EndPoint clientEndPoint;

        public UdpServer()
        {
            ProtocolType = ProtocolType.Udp;
            SocketType = SocketType.Dgram;
            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        protected override void Receive(Socket receivingSocket)
        {
            Signal.Reset();
            var state = new UdpState(receivingSocket, clientEndPoint);
            state.BeginReceive(ReadCallback);
            Signal.WaitOne();
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            Signal.Set();
            var state = (UdpState)asyncResult.AsyncState;
            state.EndReceive(asyncResult, ReadCallback, OnReceivedString);
        }
    }
}