using System;
using System.Net;
using System.Net.Sockets;

namespace TestApp
{
    internal class UdpState : StateObject
    {
        private EndPoint endPoint;

        public UdpState(Socket receiveSocket, EndPoint endPoint) : base(receiveSocket)
        {
            this.endPoint = endPoint;
        }

        public override void BeginReceive(AsyncCallback readCallback)
        {
            ReceiveSocket.BeginReceiveFrom(Buffer, 0, BufferSize, SocketFlags.None, ref endPoint, readCallback, this);
        }

        protected override void HandleFirstReceive(string str)
        {
        }

        protected override bool IsLastReceive(string str)
        {
            return true;
        }

        protected override int EndReceive(IAsyncResult asyncResult)
        {
            return ReceiveSocket.EndReceiveFrom(asyncResult, ref endPoint);
        }
    }
}