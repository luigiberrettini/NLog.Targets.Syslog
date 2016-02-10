////////////////////////////////////////////////////////////////////////////////
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
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>This class enables logging to a unix-style syslog server using NLog</summary>
    [Target("Syslog")]
    public class Syslog : TargetWithLayout
    {
        /// <summary>Gets or sets the IP Address or Host name of your Syslog server</summary>
        public string SyslogServer { get; set; }

        /// <summary>Gets or sets the port number syslog is running on (usually 514)</summary>
        public int Port { get; set; }

        /// <summary>Gets or sets the name of the application that will show up in the syslog log</summary>
        public string Sender { get; set; }

        /// <summary>Gets or sets the timestamp format</summary>
        public string TimestampFormat { get; set; }

        /// <summary>Gets or sets the machine name hosting syslog</summary>
        public string MachineName { get; set; }

        /// <summary>Gets or sets the syslog facility name to transmit message from (e.g. local0 or local7)</summary>
        public SyslogFacility Facility { get; set; }

        /// <summary>Gets or sets the syslog server protocol (TCP/UDP)</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>If this is set, try to configure and use SSL if available</summary>
        public bool Ssl { get; set; }

        /// <summary>If set, split message by newlines and send as separate messages</summary>
        public bool SplitNewlines { get; set; }

        /// <summary>Initializes a new instance of the Syslog class</summary>
        public Syslog()
        {
            SyslogServer = "127.0.0.1";
            Port = 514;
            Sender = Assembly.GetCallingAssembly().GetName().Name;
            Facility = SyslogFacility.Local1;
            Protocol = ProtocolType.Udp;
            TimestampFormat = "MMM dd HH:mm:ss";
            MachineName = Dns.GetHostName();
            SplitNewlines = true;
        }

        /// <summary>This is where we hook into NLog, by overriding the Write method</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var formattedMessageLines = GetFormattedMessageLines(logEvent);
            var severity = (SyslogSeverity)logEvent.Level;
            foreach (var formattedMessageLine in formattedMessageLines)
            {
                var message = BuildSyslogMessage(Facility, severity, DateTime.Now, Sender, formattedMessageLine);
                SendMessage(SyslogServer, Port, message, Protocol, Ssl);
            }
        }

        /// <summary>Renders message lines</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        private IEnumerable<string> GetFormattedMessageLines(LogEventInfo logEvent)
        {
            var msg = Layout.Render(logEvent);
            return SplitNewlines ? msg.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) : new[] { msg };
        }

        /// <summary>Performs the actual network part of sending a message</summary>
        /// <param name="logServer">The syslog server's host name or IP address</param>
        /// <param name="port">The UDP port that syslog is running on</param>
        /// <param name="msg">The syslog formatted message ready to transmit</param>
        /// <param name="protocol">The syslog server protocol (TCP/UDP)</param>
        /// <param name="useSsl">Specify if SSL should be used</param>
        private static void SendMessage(string logServer, int port, byte[] msg, ProtocolType protocol, bool useSsl = false)
        {
            var logServerIp = Dns.GetHostAddresses(logServer).FirstOrDefault();
            if (logServerIp == null)
                return;

            var ipAddress = logServerIp.ToString();
            switch (protocol)
            {
                case ProtocolType.Udp:
                    SendUdpMessage(port, msg, ipAddress);
                    break;
                case ProtocolType.Tcp:
                    SendTcpMessage(logServer, port, msg, useSsl, ipAddress);
                    break;
                default:
                    throw new NLogConfigurationException($"Protocol '{protocol}' is not supported");
            }
        }

        /// <summary>Performs the actual network part of sending a message with the UDP protocol</summary>
        /// <param name="port">The TCP port that syslog is running on</param>
        /// <param name="msg">The syslog formatted message ready to transmit</param>
        /// <param name="ipAddress">The syslog server's IP address</param>
        private static void SendUdpMessage(int port, byte[] msg, string ipAddress)
        {
            using (var udp = new UdpClient(ipAddress, port))
            {
                udp.Send(msg, msg.Length);
            }
        }

        /// <summary>Performs the actual network part of sending a message with the TCP protocol</summary>
        /// <param name="logServer">The syslog server's host name or IP address</param>
        /// <param name="port">The UDP port that syslog is running on</param>
        /// <param name="msg">The syslog formatted message ready to transmit</param>
        /// <param name="useSsl">Specify if SSL should be used</param>
        /// <param name="ipAddress">The syslog server's IP address</param>
        private static void SendTcpMessage(string logServer, int port, byte[] msg, bool useSsl, string ipAddress)
        {
            using (var tcp = new TcpClient(ipAddress, port))
            {
                // Disposition of tcp also disposes stream
                var stream = tcp.GetStream();
                if (useSsl)
                {
                    // Leave stream open so that we don't double dispose
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
        }

        /// <summary>Builds a syslog-compatible message using the information we have available</summary>
        /// <param name="facility">Syslog facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <param name="dateTime">Timestamp for log message</param>
        /// <param name="sender">Name of the subsystem sending the message</param>
        /// <param name="body">Message text</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage(SyslogFacility facility, SyslogSeverity severity, DateTime dateTime, string sender, string body)
        {
            var prival = CalculatePriorityValue(facility, severity).ToString(CultureInfo.InvariantCulture);
            var timestamp = dateTime.ToString(TimestampFormat, CultureInfo.GetCultureInfo("en-US"));

            return Encoding.ASCII.GetBytes($"<{prival}>{timestamp} {MachineName} {sender}: {body}{Environment.NewLine}");
        }

        /// <summary>Calculates syslog PRIVAL</summary>
        /// <param name="facility">Syslog facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private static int CalculatePriorityValue(SyslogFacility facility, SyslogSeverity severity)
        {
            return (int)facility * 8 + (int)severity;
        }
    }
}