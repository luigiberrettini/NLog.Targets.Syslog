// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>KeepAlive configuration</summary>
    /// <remarks>The number of keep-alive probes (data retransmissions) is set to 10 and cannot be changed</remarks>
    public class KeepAliveConfig : NotifyPropertyChanged
    {
        private const int DefaultTimeout = 5000;
        private const int DefaultInterval = 1000;
        private bool enabled;
        private int timeout;
        private int interval;

        /// <summary>Whether to use keep-alive or not</summary>
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value); }
        }

        /// <summary>The timeout, in milliseconds, with no activity until the first keep-alive packet is sent</summary>
        /// <remarks>The default value, on TCP socket initialization, is 2 hours</remarks>
        public int Timeout
        {
            get { return timeout; }
            set { SetProperty(ref timeout, value); }
        }

        /// <summary>The interval, in milliseconds, between when successive keep-alive packets are sent if no acknowledgement is received</summary>
        /// <remarks>The default value, on TCP socket initialization, is 1 second</remarks>
        public int Interval
        {
            get { return interval; }
            set { SetProperty(ref interval, value); }
        }

        /// <summary>Builds a new instance of the KeepAliveConfig class</summary>
        public KeepAliveConfig()
        {
            enabled = true;
            timeout = DefaultTimeout;
            interval = DefaultInterval;
        }
    }
}