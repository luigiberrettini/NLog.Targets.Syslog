// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket, int timeout)
        {
            if (timeout <= 0)
                return true;
            
            var isDisconnected = socket?.Poll(timeout, SelectMode.SelectRead) == true && socket?.Available == 0;
            return !isDisconnected;
        }
    }
}