using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TestApp
{
    internal class UdpState : StateObject
    {
        public UdpState(Socket receiveSocket) : base(receiveSocket)
        {
        }

        protected override void HandleFirstReceive(MemoryStream ms, string str)
        {
            var charsBeforePri = str.IndexOf("<", StringComparison.Ordinal);
            if (charsBeforePri != 0)
                ms.SetLength(0);
        }

        protected override void HandleLastReceive(StringBuilder receivedData, string str, Action<string> receivedStringAction)
        {
            var receivedString = receivedData.ToString();
            receivedStringAction(receivedString);
            Buffer.SetLength(0);
            receivedData.Clear();
        }
    }
}