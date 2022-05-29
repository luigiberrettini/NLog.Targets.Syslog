// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>UDP configuration</summary>
    public class UdpConfig : NotifyPropertyChanged
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;
        private Layout server;
        private int port;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public Layout Server
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

        /// <summary>Builds a new instance of the UdpProtocolConfig class</summary>
        public UdpConfig()
        {
            server = Localhost;
            port = DefaultPort;
        }
    }
}