// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForWindows : SocketInitialization
    {
        // SOCKET OPTION NAME CONSTANT
        // Ws2ipdef.h (Windows SDK)
        // #define    TCP_KEEPCNT        16
        // #define    TCP_KEEPALIVE      3
        // #define    TCP_KEEPINTVL      17

        // SOCKET OPTION DEFAULT VALUE
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd877220
        // Retry count:
        //  - 5 or min(255, max(TcpMaxDataRetransmissions, PPTPTcpMaxDataRetransmissions)) on Win 2000, Win XP and Win Server 2003
        //  - 10 on Win Vista and later and value cannot be changed before Windows 10 version 1703
        // Time: 2 hours
        // Interval: 1 second

        private const SocketOptionName TcpKeepAliveRetryCount = (SocketOptionName)0x10;
        private const SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x3;
        private const SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x11;

        private readonly bool isWin10V1703OrLater;
        private readonly bool isBelowWin10V1709;

        public SocketInitializationForWindows(Socket socket) : base(socket)
        {
            var version = Environment.OSVersion.Version;
            isWin10V1703OrLater = version.Major > 10 || version.Major == 10 && version.Build >= 15063;
            isBelowWin10V1709 = version.Major < 10 || version.Major == 10 && version.Build < 16299;
        }

        public override void DisableAddressSharing()
        {
            Socket.ExclusiveAddressUse = true;
            // DEFAULT
            // Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        }

        protected override void ApplyKeepAliveValues(KeepAliveConfig keepAliveConfig)
        {
            if (isWin10V1703OrLater)
                Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, keepAliveConfig.RetryCount);

            if (isBelowWin10V1709)
            {
                // Call WSAIoctl via IOControl
                Socket.IOControl(IOControlCode.KeepAliveValues, new IOControlKeepAliveValues(keepAliveConfig).ToByteArray(), null);
                return;
            }
            Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveConfig.Time);
            Socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveInterval, keepAliveConfig.Interval);
        }
    }
}