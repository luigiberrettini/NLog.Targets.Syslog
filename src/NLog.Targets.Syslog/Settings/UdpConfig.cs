// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>UDP configuration</summary>
    public class UdpConfig : NotifyPropertyChanged
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;
        private const int DefaultReconnectInterval = 500;
        private const int DefaultConnectionCheckTimeout = 500000;
        private string server;
        private int port;
        private TimeSpan recoveryTime;
        private int connectionCheckTimeout;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server
        {
            get => server;
            set => SetProperty(ref server, value);
        }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port
        {
            get => port;
            set => SetProperty(ref port, value);
        }

        /// <summary>The time interval, in milliseconds, after which a connection is retried</summary>
        public int ReconnectInterval
        {
            get => recoveryTime.Milliseconds;
            set => SetProperty(ref recoveryTime, TimeSpan.FromMilliseconds(value));
        }

        /// <summary>The time, in microseconds, to wait for a response when checking the connection status</summary>
        public int ConnectionCheckTimeout
        {
            get => connectionCheckTimeout;
            set => SetProperty(ref connectionCheckTimeout, value);
        }

        /// <summary>Builds a new instance of the UdpProtocolConfig class</summary>
        public UdpConfig()
        {
            server = Localhost;
            port = DefaultPort;
            recoveryTime = TimeSpan.FromMilliseconds(DefaultReconnectInterval);
            connectionCheckTimeout = DefaultConnectionCheckTimeout;
        }
    }
}