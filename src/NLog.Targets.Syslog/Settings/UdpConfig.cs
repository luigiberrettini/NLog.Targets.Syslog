// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>UDP configuration</summary>
    public class UdpConfig
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server { get; set; }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port { get; set; }

        /// <summary>Builds a new instance of the UdpProtocolConfig class</summary>
        public UdpConfig()
        {
            Server = Localhost;
            Port = DefaultPort;
        }
    }
}