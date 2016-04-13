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
    /// <summary>This class enables logging to a unix-style syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        /// <summary>Gets or sets the IP Address or Host name of your Syslog server</summary>
        public string SyslogServer { get; set; }

        /// <summary>Gets or sets the port number syslog is running on (usually 514)</summary>
        public int Port { get; set; }

        /// <summary>Gets or sets the name of the application that will show up in the syslog log</summary>
        public Layout Sender { get; set; }

        /// <summary>Gets or sets the machine name hosting syslog</summary>
        public Layout MachineName { get; set; }

        /// <summary>Gets or sets the syslog facility name to transmit message from (e.g. local0 or local7)</summary>
        public SyslogFacility Facility { get; set; }

        /// <summary>Gets or sets the syslog server protocol (TCP/UDP)</summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>If this is set, try to configure and use SSL if available</summary>
        public bool Ssl { get; set; }

        /// <summary>If set, split message by newlines and send as separate messages</summary>
        public bool SplitNewlines { get; set; }

        /// <summary>RFC number for syslog protocol</summary> 
        public RfcNumber Rfc { get; set; }

        /// <summary>RFC 3164 related fields</summary> 
        public Rfc3164 Rfc3164 { get; set; }

        /// <summary>RFC 5424 related fields</summary> 
        public Rfc5424 Rfc5424 { get; set; }

        #region RFC 5424 members

        /// <summary>Syslog protocol version for RFC 5424</summary>
        public byte ProtocolVersion { get; }

        /// <summary>Layout for PROCID protocol field</summary>
        public Layout ProcId { get; set; }

        /// <summary>Layout for MSGID protocol field</summary>
        public Layout MsgId { get; set; }

        /// <summary>Layout for STRUCTURED-DATA protocol field</summary>
        public Layout StructuredData { get; set; }

        #endregion

        /// <summary>Initializes a new instance of the Syslog class</summary>
        public SyslogTarget()
        {
            SyslogServer = "127.0.0.1";
            Port = 514;
            Sender = Assembly.GetCallingAssembly().GetName().Name;
            Facility = SyslogFacility.Local1;
            Protocol = ProtocolType.Udp;
            MachineName = Dns.GetHostName();
            SplitNewlines = true;
            Rfc = RfcNumber.Rfc3164;
            Rfc3164 = new Rfc3164(Sender, MachineName);
            Rfc5424 = new Rfc5424(Sender, MachineName);
        }

        /// <summary>
        /// Writes single event.
        /// No need to override sync version of Write(LogEventInfo) because it is called only from async version.
        /// </summary>
        /// <param name="logEvent">The NLog.AsyncLogEventInfo</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            SendEventsBatch(logEvent);
        }

        /// <summary>Writes array of events</summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            SendEventsBatch(logEvents);
        }

        /// <summary>Sends array of events to syslog server</summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        private void SendEventsBatch(params AsyncLogEventInfo[] logEvents)
        {
            var logServerIp = Dns.GetHostAddresses(SyslogServer).FirstOrDefault();
            if (logServerIp == null)
            {
                return;
            }
            var ipAddress = logServerIp.ToString();
            switch (Protocol)
            {
                case ProtocolType.Udp:
                    using (var udp = new UdpClient(ipAddress, Port))
                    {
                        ProcessAndSendEvents(logEvents, messageData => udp.Send(messageData, messageData.Length));
                    }
                    break;
                case ProtocolType.Tcp:
                    using (var tcp = new TcpClient(ipAddress, Port))
                    {
                        // disposition of tcp also disposes stream
                        var stream = tcp.GetStream();
                        if (Ssl)
                        {
                            // leave stream open so that we don't double dispose
                            using (var sslStream = new SslStream(stream, true))
                            {
                                sslStream.AuthenticateAsClient(SyslogServer);
                                ProcessAndSendEvents(logEvents, messageData => sslStream.Write(messageData, 0, messageData.Length));
                            }
                        }
                        else
                        {
                            ProcessAndSendEvents(logEvents, messageData => stream.Write(messageData, 0, messageData.Length));
                        }
                    }
                    break;
                default:
                    throw new NLogConfigurationException($"Protocol '{Protocol}' is not supported.");
            }
        }

        /// <summary>Processes array of events and sends messages bytes using action</summary>
        /// <param name="asyncLogEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <param name="messageSendAction">Implementation of send data method</param>
        private void ProcessAndSendEvents(AsyncLogEventInfo[] asyncLogEvents, Action<byte[]> messageSendAction)
        {
            asyncLogEvents
                .Select(asyncLogEvent => new SyslogLogEventInfo(asyncLogEvent.LogEvent).Build(Facility, Layout, SplitNewlines))
                .ToList()
                .ForEach(x => x.LogEntries.ForEach(line => messageSendAction(BuildMessage(x.LogEvent, x.Pri, line))));
        }

        /// <summary>Builds a syslog-compatible message using the information we have available</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            switch (Rfc)
            {
                case RfcNumber.Rfc5424:
                {
                    Rfc5424.ProtocolVersion = ProtocolVersion;
                    Rfc5424.ProcId = ProcId;
                    Rfc5424.MsgId = MsgId;
                    Rfc5424.StructuredData = StructuredData;

                    return Rfc5424.BuildMessage(logEvent, pri, logEntry);
                }
                default:
                    return Rfc3164.BuildMessage(logEvent, pri, logEntry);
            }
        }
    }
}