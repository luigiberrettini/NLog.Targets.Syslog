// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    internal class UdpServer: ServerSocket
    {
        private readonly ManualResetEvent signal;

        public UdpServer()
        {
            ProtocolType = ProtocolType.Udp;
            SocketType = SocketType.Dgram;
            signal = new ManualResetEvent(false);
        }

        protected override void Receive()
        {
            if (!KeepGoing)
                return;

            signal.Reset();
            var state = new UdpState(BoundSocket);
            state.BeginReceive(ReadCallback);
            signal.WaitOne();
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            var state = (UdpState)asyncResult.AsyncState;

            if (!KeepGoing)
            {
                state.Dispose();
                return;
            }

            signal.Set();
            state.EndReceive(asyncResult, ReadCallback, OnReceivedString);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                signal.Dispose();
            base.Dispose(disposing);
        }
    }
}