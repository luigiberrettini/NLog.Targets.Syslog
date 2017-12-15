// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Net.Sockets;

namespace FakeSyslogServer
{
    internal class TcpState : StateObject
    {
        private volatile bool firstReceive;
        private int? framing;

        public static int OctetCountingHashCode { get; }

        public static int NonTransparentHashCode { get; }

        static TcpState()
        {
            OctetCountingHashCode = "OctetCounting".GetHashCode();
            NonTransparentHashCode = "NonTransparentCounting".GetHashCode();
        }

        public TcpState(Socket receiveSocket) : base(receiveSocket)
        {
            firstReceive = true;
            framing = null;
        }

        protected override void HandleFirstReceive(MemoryStream ms)
        {
            if (!firstReceive)
                return;

            firstReceive = false;
            DetectFraming(ms);
        }

        protected override void HandleChunks(string receivedString, Action<string> receivedStringAction)
        {
            var msgSet = ServerMsgSet.FromStringAndFraming(receivedString, framing);

            if (msgSet == null)
            {
                receivedStringAction($"Error handling string {receivedString}");
                return;
            }

            for (var i = 0; i < msgSet.FullMessages; i++)
            {
                var msg = msgSet[i];
                receivedStringAction(msgSet.IsValid(msg, framing) ? msg : $"Error handling string {receivedString}");
            }

            Buffer.SetLength(0);

            if (!msgSet.LastIsPartial)
                return;

            var lastMessage = msgSet.LastMessageBytes;
            Buffer.Write(lastMessage, 0, lastMessage.Length);
        }

        private void DetectFraming(MemoryStream ms)
        {
            var firstByte = ms.GetBuffer()[0];
            var firstChar = Convert.ToChar(firstByte);

            if (char.IsDigit(firstChar))
                framing = OctetCountingHashCode;
            else if (firstChar == '<')
                framing = NonTransparentHashCode;
        }
    }
}