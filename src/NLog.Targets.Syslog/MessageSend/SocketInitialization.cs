// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using System.Runtime.InteropServices;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal abstract class SocketInitialization
    {
        protected Socket Socket { get; }

        public static SocketInitialization ForCurrentOs(Socket socket)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new SocketInitializationForWindows(socket);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new SocketInitializationForLinux(socket);
            return new SocketInitializationForOsx(socket);
        }

        protected SocketInitialization(Socket socket)
        {
            Socket = socket;
        }

        public abstract void DisableAddressSharing();

        public void DiscardPendingDataOnClose()
        {
            Socket.LingerState = new LingerOption(true, 0);
        }

        public void SetKeepAlive(KeepAliveConfig keepAliveConfig)
        {
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keepAliveConfig.Enabled);
            if (keepAliveConfig.Enabled)
                ApplyKeepAliveValues(keepAliveConfig);
        }

        protected abstract void ApplyKeepAliveValues(KeepAliveConfig keepAliveConfig);
    }
}