using System;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    internal class UdpServer: ServerSocket
    {
        private static readonly ManualResetEvent Signal = new ManualResetEvent(false);

        public UdpServer()
        {
            ProtocolType = ProtocolType.Udp;
            SocketType = SocketType.Dgram;
        }

        protected override void Receive()
        {
            if (!KeepGoing)
                return;

            Signal.Reset();
            var state = new UdpState(BoundSocket);
            state.BeginReceive(ReadCallback);
            Signal.WaitOne();
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            if (!KeepGoing)
                return;

            Signal.Set();
            var state = (UdpState)asyncResult.AsyncState;
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