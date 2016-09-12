using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    internal class TcpServer: ServerSocket
    {
        private const int DefaultListeningSocketBacklog = 1000;
        private static readonly ManualResetEvent Signal = new ManualResetEvent(false);
        private readonly int listeningSocketBacklog;

        public TcpServer(int socketBacklog = 0)
        {
            ProtocolType = ProtocolType.Tcp;
            SocketType = SocketType.Stream;
            listeningSocketBacklog = socketBacklog == 0 ? DefaultListeningSocketBacklog : socketBacklog;
        }

        protected override void SetupSocket(IPEndPoint ipEndPoint)
        {
            base.SetupSocket(ipEndPoint);
            BoundSocket.Listen(listeningSocketBacklog);
        }

        protected override void Receive()
        {
            if (!KeepGoing)
                return;

            Signal.Reset();
            BoundSocket.BeginAccept(AcceptCallback, BoundSocket);
            Signal.WaitOne();
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            if (!KeepGoing)
                return;

            Signal.Set();
            var boundSocket = (Socket)asyncResult.AsyncState;
            var receivingSocket = boundSocket.EndAccept(asyncResult);
            var state = new TcpState(receivingSocket);
            state.BeginReceive(ReadCallback);
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            if (!KeepGoing)
                return;

            var state = (TcpState)asyncResult.AsyncState;
            state.EndReceive(asyncResult, ReadCallback, OnReceivedString);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                Signal.Dispose();
        }
    }
}