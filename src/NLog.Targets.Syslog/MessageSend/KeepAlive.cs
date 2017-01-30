// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Runtime.InteropServices;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    // Win32 header file: Mstcpip.h
    //
    // struct tcp_keepalive
    // {
    //     ULONG onoff;
    //     ULONG keepalivetime;
    //     ULONG keepaliveinterval;
    // };
    internal class KeepAlive
    {
        private readonly int onOffOffset;
        private readonly int timeOffset;
        private readonly int intervalOffset;
        private readonly int structSize;
        private readonly uint onOff;
        private readonly uint time;
        private readonly uint interval;

        public KeepAlive(KeepAliveConfig keepAliveConfig)
        {
            var uintSize = Marshal.SizeOf(typeof(uint));
            onOffOffset = 0;
            timeOffset = uintSize;
            intervalOffset = 2 * uintSize;
            structSize = 3 * uintSize;
            onOff = (uint)(keepAliveConfig.Enabled ? 1 : 0);
            time = (uint)keepAliveConfig.Timeout;
            interval = (uint)keepAliveConfig.Interval;
        }

        public byte[] ToByteArray()
        {
            var keepAliveSettings = new byte[structSize];
            BitConverter.GetBytes(onOff).CopyTo(keepAliveSettings, onOffOffset);
            BitConverter.GetBytes(time).CopyTo(keepAliveSettings, timeOffset);
            BitConverter.GetBytes(interval).CopyTo(keepAliveSettings, intervalOffset);
            return keepAliveSettings;
        }
    }
}