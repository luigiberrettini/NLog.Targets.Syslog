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

using NLog.Common;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        /// <summary>The IP Address or Host name of your Syslog server</summary>
        public string SyslogServer { get; set; }

        /// <summary>The port number Syslog is running on (usually 514)</summary>
        public int Port { get; set; }

        /// <summary>The name of the application that will show up in the Syslog log</summary>
        public Layout Sender { get; set; }

        /// <summary>The name of the machine to log from</summary>
        public Layout MachineName { get; set; }

        /// <summary>The Syslog facility name to log from (e.g. local0 or local7)</summary>
        public SyslogFacility Facility { get; set; }

        /// <summary>The Syslog server protocol (TCP/UDP)</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>Whether to use SSL or not (TCP only)</summary>
        public bool Ssl { get; set; }

        /// <summary>Whether or not to split each log entry by newlines and send each line separately</summary>
        public bool SplitNewlines { get; set; }

        /// <summary>The Syslog protocol RFC to be followed</summary> 
        public RfcNumber Rfc { get; set; }

        /// <summary>RFC 3164 related fields</summary> 
        public Rfc3164 Rfc3164 { get; set; }

        /// <summary>RFC 5424 related fields</summary> 
        public Rfc5424 Rfc5424 { get; set; }

        /// <summary>Initializes a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            SyslogServer = "127.0.0.1";
            Port = 514;
            Sender = Assembly.GetCallingAssembly().GetName().Name;
            MachineName = Dns.GetHostName();
            Facility = SyslogFacility.Local1;
            Protocol = ProtocolType.Udp;
            Ssl = false;
            SplitNewlines = true;
            Rfc = RfcNumber.Rfc3164;
            Rfc3164 = new Rfc3164(Sender, MachineName);
            Rfc5424 = new Rfc5424(Sender, MachineName);
        }

        /// <summary>Writes a single event</summary>
        /// <param name="logEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to ovveride it</remarks>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            SendEvents(logEvent);
        }

        /// <summary>Writes array of events</summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to ovveride it</remarks>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            SendEvents(logEvents);
        }

        /// <summary>Sends array of events to Syslog server</summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        private void SendEvents(params AsyncLogEventInfo[] logEvents)
        {
            var logServerIp = Dns.GetHostAddresses(SyslogServer).FirstOrDefault();
            if (logServerIp == null)
                return;
            var ipAddress = logServerIp.ToString();

            switch (Protocol)
            {
                case ProtocolType.Udp:
                {
                    using (var udp = new UdpClient(ipAddress, Port))
                    {
                        SendMessages(logEvents, messageData => udp.Send(messageData, messageData.Length));
                    }
                    break;
                }
                case ProtocolType.Tcp:
                {
                    using (var tcp = new TcpClient(ipAddress, Port))
                    {
                        // TcpClient disposes also the stream
                        var stream = tcp.GetStream();
                        if (Ssl)
                        {
                            // To avoid a double dispose leaveInnerStreamOpen is set to true
                            using (var sslStream = new SslStream(stream, true))
                            {
                                sslStream.AuthenticateAsClient(SyslogServer);
                                SendMessages(logEvents, messageData => sslStream.Write(messageData, 0, messageData.Length));
                            }
                        }
                        else
                        {
                            SendMessages(logEvents, messageData => stream.Write(messageData, 0, messageData.Length));
                        }
                    }
                    break;
                }
                default:
                {
                    throw new NLogConfigurationException($"Protocol '{Protocol}' is not supported.");
                }
            }
        }

        /// <summary>Processes array of events and sends messages bytes using action</summary>
        /// <param name="asyncLogEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <param name="send">Implementation of the send method</param>
        private void SendMessages(IEnumerable<AsyncLogEventInfo> asyncLogEvents, Action<byte[]> send)
        {
            asyncLogEvents
                .Select(asyncLogEvent => new SyslogLogEventInfo(asyncLogEvent.LogEvent).Build(Facility, Layout, SplitNewlines))
                .ToList()
                .ForEach(x => x.LogEntries.ForEach(line => send(BuildMessage(x.LogEvent, x.Pri, line))));
        }

        /// <summary>Builds a Syslog-compatible message using the information we have available</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Byte array containing formatted Syslog message</returns>
        private byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            switch (Rfc)
            {
                case RfcNumber.Rfc3164:
                {
                    return Rfc3164.BuildMessage(logEvent, pri, logEntry);
                }
                case RfcNumber.Rfc5424:
                {
                    return Rfc5424.BuildMessage(logEvent, pri, logEntry);
                }
                default:
                {
                    throw new NLogConfigurationException($"RFC '{Rfc}' is not supported.");
                }
            }
        }
    }
}