// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForOsx : SocketInitialization
    {
        // SOCKET OPTION NAME CONSTANT
        // https://opensource.apple.com/source/xnu/xnu-4570.41.2/bsd/sys/socket.h.auto.html
        // #define    SO_REUSEPORT       0x0200                    /* allow local address & port reuse */
        // https://opensource.apple.com/source/xnu/xnu-4570.41.2/bsd/netinet/tcp.h.auto.html
        // #define    TCP_KEEPCNT        0x102                     /* number of keepalives before close */
        // #define    TCP_KEEPALIVE      0x10                      /* idle time used when SO_KEEPALIVE is enabled */
        // #define    TCP_KEEPINTVL      0x101                     /* interval between keepalives */

        // SOCKET OPTION DEFAULT VALUE
        // https://opensource.apple.com/source/xnu/xnu-4570.41.2/bsd/netinet/tcp_timer.h.auto.html
        // #define    TCPTV_KEEPCNT      8                         /* max probes before drop */
        // #define    TCPTV_KEEP_IDLE    (120*60*TCP_RETRANSHZ)    /* time before probing */
        // #define    TCPTV_KEEPINTVL    (75*TCP_RETRANSHZ)        /* default probe interval */

        // private const SocketOptionName SocketReusePort = (SocketOptionName)0x0200;
        private const SocketOptionName TcpKeepAliveRetryCount = (SocketOptionName)0x102;
        private const SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x10;
        private const SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x101;

        public override void DisableAddressSharing(Socket socket)
        {
            // DEFAULT
            // Interop.SetSockOptSysCall(socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            // Interop.SetSockOptSysCall(socket, SocketOptionLevel.Socket, SocketReusePort, 0);
        }

        protected override void ApplyKeepAliveValues(Socket socket, KeepAliveConfig keepAliveConfig)
        {
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, keepAliveConfig.RetryCount);
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveConfig.Time);
            Interop.SetSockOptSysCall(socket, SocketOptionLevel.Tcp, TcpKeepAliveInterval, keepAliveConfig.Interval);
        }
    }
}