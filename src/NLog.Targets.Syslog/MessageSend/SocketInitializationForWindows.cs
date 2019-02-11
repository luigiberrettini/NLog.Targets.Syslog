// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class SocketInitializationForWindows : SocketInitialization
    {
        private const int DefaultRetryCount = 10;
        private const int DefaultTime = 7200;
        private const int DefaultInterval = 1;
        private static readonly bool CanSetSocketOptionKeepAliveRetryCount = CanSetSockOptKeepAliveRetryCount();
        private static readonly bool CanSetSocketOptionsKeepAliveTimeAndInterval = CanSetSockOptsKeepAliveTimeAndInterval();

        [ThreadStatic]
        private static KeepAliveConfig keepAliveConfiguration;

        [ThreadStatic]
        private static byte[] ioControlKeepAliveValues;

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

        public override void DisableAddressSharing(Socket socket)
        {
            socket.ExclusiveAddressUse = true;
            // DEFAULT
            // socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        }

        protected override void ApplyKeepAliveValues(Socket socket, KeepAliveConfig keepAliveConfig)
        {
            if (CanSetSocketOptionKeepAliveRetryCount)
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, keepAliveConfig.RetryCount);

            if (CanSetSocketOptionsKeepAliveTimeAndInterval)
            {
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveConfig.Time);
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveInterval, keepAliveConfig.Interval);
                return;
            }

            // Call WSAIoctl via IOControl
            if (KeepAliveConfigurationIsUpToDate(keepAliveConfig) && ioControlKeepAliveValues != null)
            {
                socket.IOControl(IOControlCode.KeepAliveValues, ioControlKeepAliveValues, null);
                return;
            }
            keepAliveConfiguration = keepAliveConfig;
            var buffer = ioControlKeepAliveValues ?? (ioControlKeepAliveValues = new byte[3 * sizeof(uint)]);
            BitConverter.GetBytes(keepAliveConfig.Enabled ? 1u : 0u).CopyTo(buffer, 0);
            BitConverter.GetBytes((uint)keepAliveConfig.Time * 1000).CopyTo(buffer, sizeof(uint));
            BitConverter.GetBytes((uint)keepAliveConfig.Interval * 1000).CopyTo(buffer, sizeof(uint) * 2);
            socket.IOControl(IOControlCode.KeepAliveValues, buffer, null);
        }

        private static bool CanSetSockOptKeepAliveRetryCount()
        {
            return CanSetSockOptKeepAliveSetting(socket =>
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveRetryCount, DefaultRetryCount));
        }

        private static bool CanSetSockOptsKeepAliveTimeAndInterval()
        {
            return CanSetSockOptKeepAliveSetting(socket =>
            {
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveTime, DefaultTime);
                socket.SetSocketOption(SocketOptionLevel.Tcp, TcpKeepAliveInterval, DefaultInterval);
            });
        }

        private static bool CanSetSockOptKeepAliveSetting(Action<Socket> setKeepAliveSetting)
        {
            using (var tcp = new TcpClient())
            {
                try
                {
                    setKeepAliveSetting(tcp.Client);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool KeepAliveConfigurationIsUpToDate(KeepAliveConfig keepAliveConfig)
        {
            return keepAliveConfiguration.Enabled == keepAliveConfig.Enabled &&
                keepAliveConfiguration.RetryCount == keepAliveConfig.RetryCount &&
                keepAliveConfiguration.Time == keepAliveConfig.Time &&
                keepAliveConfiguration.Interval == keepAliveConfig.Interval;
        }
    }
}