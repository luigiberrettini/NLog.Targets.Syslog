// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Runtime.InteropServices;

namespace NLog.Targets.Syslog.Settings
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
        private readonly int uintSize;
        private readonly int onOffOffset;
        private readonly int timeOffset;
        private readonly int intervalOffset;
        private readonly int structSize;

        public uint OnOff { get; }

        public uint Time { get; }

        public uint Interval { get; }

        public KeepAlive(KeepAliveConfig keepAliveConfig)
        {
            uintSize = Marshal.SizeOf(typeof(uint));
            onOffOffset = 0;
            timeOffset = uintSize;
            intervalOffset = 2 * uintSize;
            structSize = 3 * uintSize;
            OnOff = (uint)(keepAliveConfig.Enabled ? 1 : 0);
            Time = (uint)keepAliveConfig.Timeout;
            Interval = (uint)keepAliveConfig.Interval;
        }
        
        public byte[] ToByteArray()
        {
            byte[] keepAliveSettings = new byte[structSize];
            BitConverter.GetBytes(OnOff).CopyTo(keepAliveSettings, onOffOffset);
            BitConverter.GetBytes(Time).CopyTo(keepAliveSettings, timeOffset);
            BitConverter.GetBytes(Interval).CopyTo(keepAliveSettings, intervalOffset);
            return keepAliveSettings;
        }
    }
}