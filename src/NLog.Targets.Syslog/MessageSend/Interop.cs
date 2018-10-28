using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NLog.Targets.Syslog.MessageSend
{
    internal static class Interop
    {
        internal static void SetSockOptSysCall (Socket socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            unsafe
            {
                ThrowOnError(SetSockOptSysCall(socket.Handle, (int)optionLevel, (int)optionName, (byte*)&optionValue, sizeof(int)));
            }
        }

        [DllImport("libc", EntryPoint = "setsockopt")]
        private static extern unsafe int SetSockOptSysCall(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int optionLen);

        private static void ThrowOnError(int error)
        {
            if (error != 0)
                throw new ApplicationException($"Socket option syscall error value: '{error}'");
        }
    }
}