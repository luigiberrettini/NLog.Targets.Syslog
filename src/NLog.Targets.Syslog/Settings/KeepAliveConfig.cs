// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>KeepAlive configuration</summary>
    public class KeepAliveConfig : NotifyPropertyChanged
    {
        private const int DefaultRetryCount = 10;
        private const int DefaultTime = 5;
        private const int DefaultInterval = 1;
        private bool enabled;
        private int retryCount;
        private int time;
        private int interval;

        /// <summary>Whether to use keep-alive or not</summary>
        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }

        /// <summary>The number of unacknowledged keep-alive probes to send before considering the connection dead and terminating it</summary>
        /// <remarks>
        ///     The default value, on TCP socket initialization is:
        ///      - 5 on Windows 2000, Windows XP and Windows Server 2003
        ///        Can be changed editing the registry and is min(255, max(TcpMaxDataRetransmissions, PPTPTcpMaxDataRetransmissions))
        ///      - 10 on Windows Vista and later
        ///        Can be changed only on Windows 10 V 1703 and later via SetSocketOption
        ///      - 9 on Linux
        ///      - 8 on OSX
        /// </remarks>
        public int RetryCount
        {
            get => retryCount;
            set => SetProperty(ref retryCount, value);
        }

        /// <summary>The number of seconds a connection will remain idle before the first keep-alive probe is sent</summary>
        /// <remarks>
        ///     No more used after the connection has been marked to need keep-alive
        ///     The default value, on TCP socket initialization, is 2 hours
        /// </remarks>
        public int Time
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        /// <summary>The number of seconds a connection will wait for a keep-alive acknowledgement before sending another keepalive probe</summary>
        /// <remarks>
        ///     The default value, on TCP socket initialization, is:
        ///      - 1 second on Windows
        ///      - 75 seconds on Linux
        ///      - 75 second on OSX
        /// </remarks>
        public int Interval
        {
            get => interval;
            set => SetProperty(ref interval, value);
        }

        /// <summary>Builds a new instance of the KeepAliveConfig class</summary>
        public KeepAliveConfig()
        {
            enabled = true;
            retryCount = DefaultRetryCount;
            time = DefaultTime;
            interval = DefaultInterval;
        }
    }
}