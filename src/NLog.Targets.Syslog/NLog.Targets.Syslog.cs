////
//   NLog.Targets.Syslog
//   ------------------------------------------------------------------------
//   Copyright 2013 Jesper Hess Nielsen <jesper@graffen.dk>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
////

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;
    using System.Globalization;
    using System.Net.Security;
    using System.Collections.Generic;

    /// <summary>
    /// This class enables logging to a unix-style syslog server using NLog.
    /// </summary>
    [Target("Syslog")]
    public class Syslog : TargetWithLayout
    {
        /// <summary>
        /// Gets or sets the IP Address or Host name of your Syslog server
        /// </summary>
        public string SyslogServer { get; set; }

        /// <summary>
        /// Gets or sets the Port number syslog is running on (usually 514)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the application that will show up in the syslog log
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the timestamp format
        /// </summary>
        public string TimestampFormat { get; set; }

        /// <summary>
        /// Gets or sets the machine name hosting syslog
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the syslog facility name to send messages as (for example, local0 or local7)
        /// </summary>
        public SyslogFacility Facility { get; set; }

        /// <summary>
        /// Gets or sets the syslog server protocol (tcp/udp) 
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// If this is set, try to configure and use SSL if available.
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// If set, split message by newlines and send as separate messages
        /// </summary>
        public bool SplitNewlines { get; set; }

        /// <summary>
        /// Initializes a new instance of the Syslog class
        /// </summary>
        public Syslog()
        {
            // Sensible defaults...
            this.SyslogServer = "127.0.0.1";
            this.Port = 514;
            this.Sender = Assembly.GetCallingAssembly().GetName().Name;
            this.Facility = SyslogFacility.Local1;
            this.Protocol = ProtocolType.Udp;
            this.TimestampFormat = "MMM dd HH:mm:ss ";
            this.MachineName = Dns.GetHostName();
            this.SplitNewlines = true;
        }

        /// <summary>
        /// This is where we hook into NLog, by overriding the Write method. 
        /// </summary>
        /// <param name="logEvent">The NLog.LogEventInfo </param>
        protected override void Write(LogEventInfo logEvent)
        {
            // Store the current UI culture
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            // Set the current Locale to "en-US" for proper date formatting
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var formattedMessageLines = this.GetFormattedMessageLines(logEvent);
            var severity = GetSyslogSeverity(logEvent.Level);
            foreach (var formattedMessageLine in formattedMessageLines)
            {
                var message = this.BuildSyslogMessage(this.Facility, severity, DateTime.Now, this.Sender, formattedMessageLine);
                SendMessage(this.SyslogServer, this.Port, message, this.Protocol, this.Ssl);
            }

            // Restore the original culture
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }

        private IEnumerable<string> GetFormattedMessageLines(LogEventInfo logEvent)
        {
            var msg = this.Layout.Render(logEvent);
            return this.SplitNewlines ? msg.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) : new[] { msg };
        }

        /// <summary>
        /// Performs the actual network part of sending a message
        /// </summary>
        /// <param name="logServer">The syslog server's host name or IP address</param>
        /// <param name="port">The UDP port that syslog is running on</param>
        /// <param name="msg">The syslog formatted message ready to transmit</param>
        /// <param name="protocol">The syslog server protocol (tcp/udp)</param>
        /// <param name="useSsl">Specify if SSL should be used</param>
        private static void SendMessage(string logServer, int port, byte[] msg, ProtocolType protocol, bool useSsl = false)
        {
            var logServerIp = Dns.GetHostAddresses(logServer).FirstOrDefault();
            if (logServerIp == null)
            {
                return;
            }

            var ipAddress = logServerIp.ToString();
            switch (protocol)
            {
                case ProtocolType.Udp:
                    using (var udp = new UdpClient(ipAddress, port))
                    {
                        udp.Send(msg, msg.Length);
                    }
                    break;
                case ProtocolType.Tcp:
                    using (var tcp = new TcpClient(ipAddress, port))
                    {
                        // disposition of tcp also disposes stream
                        var stream = tcp.GetStream();
                        if (useSsl)
                        {
                            // leave stream open so that we don't double dispose
                            using (var sslStream = new SslStream(stream, true))
                            {
                                sslStream.AuthenticateAsClient(logServer);
                                sslStream.Write(msg, 0, msg.Length);
                            }
                        }
                        else
                        {
                            stream.Write(msg, 0, msg.Length);
                        }
                    }

                    break;
                default:
                    throw new NLogConfigurationException($"Protocol '{protocol}' is not supported.");
            }
        }

        /// <summary>
        /// Mapping between NLog levels and syslog severity levels as they are not exactly one to one. 
        /// </summary>
        /// <param name="logLevel">NLog log level to translate</param>
        /// <returns>SyslogSeverity which corresponds to the NLog level. </returns>
        private static SyslogSeverity GetSyslogSeverity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Fatal)
            {
                return SyslogSeverity.Emergency;
            }

            if (logLevel >= LogLevel.Error)
            {
                return SyslogSeverity.Error;
            }

            if (logLevel >= LogLevel.Warn)
            {
                return SyslogSeverity.Warning;
            }

            if (logLevel >= LogLevel.Info)
            {
                return SyslogSeverity.Informational;
            }

            if (logLevel >= LogLevel.Debug)
            {
                return SyslogSeverity.Debug;
            }

            return SyslogSeverity.Notice;
        }

        /// <summary>
        /// Builds a syslog-compatible message using the information we have available. 
        /// </summary>
        /// <param name="facility">Syslog Facility to transmit message from</param>
        /// <param name="priority">Syslog severity level</param>
        /// <param name="time">Time stamp for log message</param>
        /// <param name="sender">Name of the subsystem sending the message</param>
        /// <param name="body">Message text</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage(SyslogFacility facility, SyslogSeverity priority, DateTime time, string sender, string body)
        {
            // Get sender machine name
            var machine = this.MachineName + " ";

            // Calculate PRI field
            var calculatedPriority = (int)facility * 8 + (int)priority;
            var pri = "<" + calculatedPriority.ToString(CultureInfo.InvariantCulture) + ">";

            var timeToString = time.ToString(this.TimestampFormat);
            sender = sender + ": ";

            string[] strParams = { pri, timeToString, machine, sender, body, Environment.NewLine };
            return Encoding.ASCII.GetBytes(string.Concat(strParams));
        }
    }
}
