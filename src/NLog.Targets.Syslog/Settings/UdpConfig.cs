// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>UDP configuration</summary>
    public class UdpConfig : NotifyPropertyChanged
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;
        private string server;
        private int port;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server
        {
            get { return server; }
            set { SetProperty(ref server, value); }
        }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port
        {
            get { return port; }
            set { SetProperty(ref port, value); }
        }

        /// <summary>Builds a new instance of the UdpProtocolConfig class</summary>
        public UdpConfig()
        {
            server = Localhost;
            port = DefaultPort;
        }
    }
}