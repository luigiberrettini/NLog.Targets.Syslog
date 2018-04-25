// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForOsx : SocketInitialization
    {
        // SOCKET OPTION NAME CONSTANT
        // https://opensource.apple.com/source/xnu/xnu-4570.31.3/bsd/netinet/tcp.h.auto.html
        // #define    TCP_KEEPCNT        0x102                     /* number of keepalives before close */
        // #define    TCP_KEEPALIVE      0x10                      /* idle time used when SO_KEEPALIVE is enabled */
        // #define    TCP_KEEPINTVL      0x101                     /* interval between keepalives */

        // SOCKET OPTION DEFAULT VALUE
        // https://opensource.apple.com/source/xnu/xnu-4570.31.3/bsd/netinet/tcp_timer.h.auto.html
        // #define    TCPTV_KEEPCNT      8                         /* max probes before drop */
        // #define    TCPTV_KEEP_IDLE    (120*60*TCP_RETRANSHZ)    /* time before probing */
        // #define    TCPTV_KEEPINTVL    (75*TCP_RETRANSHZ)        /* default probe interval */

        private const SocketOptionName TcpKeepAliveRetryCount = (SocketOptionName)0x102;
        private const SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x10;
        private const SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x101;

        public SocketInitializationForOsx(Socket socket) : base(socket)
        {
        }

        public override void EnableExclusiveAddressUse()
        {
            #if NETSTANDARD2_1
            Socket.ExclusiveAddressUse = true;
            #endif
        }

        protected override void ApplyKeepAliveValues(KeepAliveConfig keepAliveConfig)
        {
            Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, keepAliveConfig.RetryCount);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveConfig.Time);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveInterval, keepAliveConfig.Interval);
        }
    }
}