// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForLinux : SocketInitialization
    {
        // SOCKET OPTION NAME CONSTANT
        // https://github.com/torvalds/linux/blob/v4.16/include/net/tcp.h
        // #define    TCP_KEEPCNT             6              /* Number of keepalives before death */
        // #define    TCP_KEEPIDLE            4              /* Start keeplives after this period */
        // #define    TCP_KEEPINTVL           5              /* Interval between keepalives */

        // SOCKET OPTION DEFAULT VALUE
        // https://github.com/torvalds/linux/blob/v4.16/include/net/tcp.h
        // #define    TCP_KEEPALIVE_PROBES    9              /* Max of 9 keepalive probes    */
        // #define    TCP_KEEPALIVE_TIME      (120*60*HZ)    /* two hours */
        // #define    TCP_KEEPALIVE_INTVL     (75*HZ)

        private const SocketOptionName TcpKeepAliveRetryCount = (SocketOptionName)0x6;   // TCP_KEEPCNT
        private const SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x4;         // TCP_KEEPIDLE
        private const SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x5;     // TCP_KEEPINTVL

        public SocketInitializationForLinux(Socket socket) : base(socket)
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