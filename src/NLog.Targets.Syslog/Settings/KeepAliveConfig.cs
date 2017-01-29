// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>KeepAlive configuration</summary>
    /// <remarks>The number of keep-alive probes (data retransmissions) is set to 10 and cannot be changed</remarks>
    public class KeepAliveConfig
    {
        private const int DefaultTimeout = 500;
        private const int DefaultInterval = 100;

        /// <summary>Whether to use keep-alive or not</summary>
        public bool Enabled { get; set; }

        /// <summary>The timeout, in milliseconds, with no activity until the first keep-alive packet is sent</summary>
        /// <remarks>The default value, on TCP socket initialization, is 2 hours</remarks>
        public int Timeout { get; set; }

        /// <summary>The interval, in milliseconds, between when successive keep-alive packets are sent if no acknowledgement is received</summary>
        /// <remarks>The default value, on TCP socket initialization, is 1 second</remarks>
        public int Interval { get; set; }

        public KeepAliveConfig()
        {
            Enabled = true;
            Timeout = DefaultTimeout;
            Interval = DefaultInterval;
        }
    }
}