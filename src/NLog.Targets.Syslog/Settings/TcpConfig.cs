// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>TCP configuration</summary>
    public class TcpConfig
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;
        private const int DefaultReconnectInterval = 500;
        private const int DefaultConnectionCheckTimeout = 500000;
        private const int DefaultBufferSize = 4096;
        private FramingMethod framing;
        private TimeSpan recoveryTime;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server { get; set; }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port { get; set; }

        /// <summary>The time interval, in milliseconds, after which a connection is retried</summary>
        public int ReconnectInterval
        {
            get { return recoveryTime.Milliseconds; }
            set { recoveryTime = TimeSpan.FromMilliseconds(value); }
        }

        /// <summary>KeepAlive configuration</summary>
        public KeepAliveConfig KeepAlive { get; set; }

        /// <summary>The time, in microseconds, to wait for a response when checking the connection status</summary>
        public int ConnectionCheckTimeout { get; set; }

        /// <summary>Whether to use TLS or not (TLS 1.2 only)</summary>
        public bool UseTls { get; set; }

        /// <summary>Which framing method to use</summary>
        /// <remarks>If <see cref="UseTls">is true</see> get will always return OctetCounting (RFC 5425)</remarks>
        public FramingMethod Framing
        {
            get { return UseTls ? FramingMethod.OctetCounting : framing; }
            set { framing = value; }
        }

        /// <summary>The size of chunks in which data is split to be sent over the wire</summary>
        public int DataChunkSize { get; set; }

        /// <summary>Builds a new instance of the TcpProtocolConfig class</summary>
        public TcpConfig()
        {
            Server = Localhost;
            Port = DefaultPort;
            ReconnectInterval = DefaultReconnectInterval;
            KeepAlive = new KeepAliveConfig();
            ConnectionCheckTimeout = DefaultConnectionCheckTimeout;
            UseTls = true;
            Framing = FramingMethod.OctetCounting;
            DataChunkSize = DefaultBufferSize;
        }
    }
}