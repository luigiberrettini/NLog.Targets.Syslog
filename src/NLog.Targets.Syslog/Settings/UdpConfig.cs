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
        private string server;
        private int port;
        private int reconnectInterval;

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
            get => reconnectInterval;
            set => SetProperty(ref reconnectInterval, value <= 0 ? DefaultReconnectInterval : value);
        }

        /// <summary>Builds a new instance of the UdpProtocolConfig class</summary>
        public UdpConfig()
        {
            server = Localhost;
            port = DefaultPort;
            reconnectInterval = DefaultReconnectInterval;
        }
    }
}