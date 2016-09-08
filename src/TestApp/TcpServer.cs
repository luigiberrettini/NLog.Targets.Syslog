using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    internal class TcpServer: ServerSocket
    {
        private const int DefaultListeningSocketBacklog = 100;
        private static readonly ManualResetEvent Signal = new ManualResetEvent(false);
        private readonly int listeningSocketBacklog;

        public TcpServer(int socketBacklog = 0)
        {
            ProtocolType = ProtocolType.Tcp;
            SocketType = SocketType.Stream;
            listeningSocketBacklog = socketBacklog == 0 ? DefaultListeningSocketBacklog : socketBacklog;
        }

        protected override void SetupSocket(IPEndPoint ipEndPoint, Socket boundSocket)
        {
            base.SetupSocket(ipEndPoint, boundSocket);
            boundSocket.Listen(listeningSocketBacklog);
        }

        protected override void Receive(Socket receivingSocket)
        {
            Signal.Reset();
            receivingSocket.BeginAccept(AcceptCallback, receivingSocket);
            Signal.WaitOne();
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            Signal.Set();
            var boundSocket = (Socket)asyncResult.AsyncState;
            var receivingSocket = boundSocket.EndAccept(asyncResult);
            var state = new TcpState(receivingSocket);
            state.BeginReceive(ReadCallback);
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            var state = (TcpState)asyncResult.AsyncState;
            state.EndReceive(asyncResult, ReadCallback, OnReceivedString);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Signal.Dispose();
            base.Dispose(disposing);
        }
    }
}