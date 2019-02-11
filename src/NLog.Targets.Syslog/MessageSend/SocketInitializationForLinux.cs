// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForLinux : SocketInitialization
    {
        // SOCKET OPTION NAME CONSTANT
        // https://github.com/torvalds/linux/blob/v4.16/include/uapi/asm-generic/socket.h
        // #define    SO_REUSEPORT           15
        // https://github.com/torvalds/linux/blob/v4.16/include/net/tcp.h
        // #define    TCP_KEEPCNT             6              /* Number of keepalives before death */
        // #define    TCP_KEEPIDLE            4              /* Start keeplives after this period */
        // #define    TCP_KEEPINTVL           5              /* Interval between keepalives */

        // SOCKET OPTION DEFAULT VALUE
        // https://github.com/torvalds/linux/blob/v4.16/include/net/tcp.h
        // #define    TCP_KEEPALIVE_PROBES    9              /* Max of 9 keepalive probes    */
        // #define    TCP_KEEPALIVE_TIME      (120*60*HZ)    /* two hours */
        // #define    TCP_KEEPALIVE_INTVL     (75*HZ)

        // private const SocketOptionName SocketReusePort = (SocketOptionName)15;
        private const SocketOptionName TcpKeepAliveRetryCount = (SocketOptionName)0x6;
        private const SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x4;
        private const SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x5;

        public override void DisableAddressSharing(Socket socket)
        {
            // DEFAULT
            // Interop.SetSockOptSysCall(socket, SocketOptionLevel.Socket, SocketReusePort, 0);
            // Interop.SetSockOptSysCall(socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        }

        protected override void ApplyKeepAliveValues(Socket socket, KeepAliveConfig keepAliveConfig)
        {
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, keepAliveConfig.RetryCount);
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveConfig.Time);
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveInterval, keepAliveConfig.Interval);
        }
    }
}