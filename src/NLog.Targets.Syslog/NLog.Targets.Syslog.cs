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
using System.Runtime.CompilerServices;
using System.Text;
using NLog.Common;
using NLog.Layouts;

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
        private const string NilValue = "-";
        private static readonly CultureInfo _usCulture = new CultureInfo("en-US");
        private static readonly byte[] _bom = { 0xEF, 0xBB, 0xBF };
        private static readonly char[] _lineSeps = { '\r', '\n' };

        /// <summary>Gets or sets the IP Address or Host name of your Syslog server</summary>
        public string SyslogServer { get; set; }

        /// <summary>Gets or sets the port number syslog is running on (usually 514)</summary>
        public int Port { get; set; }

        /// <summary>Gets or sets the name of the application that will show up in the syslog log</summary>
        public Layout Sender { get; set; }

        /// <summary>Gets or sets the timestamp format</summary>
        public string TimestampFormat { get; set; }

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

        #region RFC 5424 members

        /// <summary>Syslog protocol version for RFC 5424</summary>
        private byte ProtocolVersion { get; }

        /// <summary>Layout for PROCID protocol field</summary>
        public Layout ProcId { get; set; }

        /// <summary>Layout for MSGID protocol field</summary>
        public Layout MsgId { get; set; }

        /// <summary>Layout for STRUCTURED-DATA protocol field</summary>
        public Layout StructuredData { get; set; }

        #endregion

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
            Rfc = RfcNumber.Rfc3164;

            //Defaults for rfc 5424
            ProtocolVersion = 1;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = NilValue;
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
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <param name="messageSendAction">Implementation of send data method</param>
        void ProcessAndSendEvents(AsyncLogEventInfo[] logEvents, Action<byte[]> messageSendAction)
        {
            foreach (var asyncLogEvent in logEvents)
            {
                var logEvent = asyncLogEvent.LogEvent;
                var formattedMessageLines = FormatMessageLines(logEvent);
                var severity = (SyslogSeverity)logEvent.Level;
                foreach (var formattedMessageLine in formattedMessageLines)
                {
                    var message = BuildSyslogMessage(logEvent, Facility, severity, formattedMessageLine);
                    messageSendAction(message);
                }
            }
        }

        /// <summary>Builds a syslog-compatible message using the information we have available</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="facility">Syslog Facility to transmit message from</param>
        /// <param name="priority">Syslog severity level</param>
        /// <param name="body">Message text</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage(LogEventInfo logEvent, SyslogFacility facility, SyslogSeverity priority, string body)
        {
            switch (Rfc)
            {
                case RfcNumber.Rfc5424:
                    return BuildSyslogMessage5424(logEvent, facility, priority, body);
                default:
                    return BuildSyslogMessage3164(logEvent, facility, priority, body);
            }
        }

        /// <summary>Builds rfc-3164 compatible message</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="facility">Syslog Facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <param name="body">Message text</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage3164(LogEventInfo logEvent, SyslogFacility facility, SyslogSeverity severity, string body)
        {
            // Calculate PRI field
            var priority = CalculatePriorityValue(facility, severity).ToString(CultureInfo.InvariantCulture);
            var time = logEvent.TimeStamp.ToString(TimestampFormat, _usCulture);
            // Get sender machine name
            var machine = MachineName.Render(logEvent);
            // Get sender
            var sender = Sender.Render(logEvent);

            return Encoding.ASCII.GetBytes($"<{priority}>{time} {machine} {sender}: {body}{Environment.NewLine}");
        }

        /// <summary>Builds rfc-5424 compatible message</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="facility">Syslog Facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <param name="body">Message text</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage5424(LogEventInfo logEvent, SyslogFacility facility, SyslogSeverity severity, string body)
        {
            // Calculate PRI field
            var priority = CalculatePriorityValue(facility, severity).ToString(CultureInfo.InvariantCulture);
            var version = ProtocolVersion.ToString(CultureInfo.InvariantCulture);
            var time = logEvent.TimeStamp.ToString("o");
            // Get sender machine name
            var machine = Left(MachineName.Render(logEvent), 255);
            var sender = Left(Sender.Render(logEvent), 48);
            var procId = Left(ProcId.Render(logEvent), 128);
            var msgId = Left(MsgId.Render(logEvent), 32);

            var headerData = Encoding.ASCII.GetBytes($"<{priority}>{version} {time} {machine} {sender} {procId} {msgId} ");
            var structuredData = Encoding.UTF8.GetBytes(StructuredData.Render(logEvent) + " ");
            var messageData = Encoding.UTF8.GetBytes(body);

            var allData = new List<byte>(headerData.Length + structuredData.Length + _bom.Length + messageData.Length);
            allData.AddRange(headerData);
            allData.AddRange(structuredData);
            allData.AddRange(_bom);
            allData.AddRange(messageData);
            return allData.ToArray();
        }

        /// <summary>Gets at most length first symbols</summary>
        /// <param name="value">Source string</param>
        /// <param name="length">Maximum symbols count</param>
        /// <returns>String that contains at most length symbols</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Left(string value, int length)
        {
            return value.Length <= length ? value : value.Substring(0, length);
        }

        /// <summary>Renders message lines</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        private IEnumerable<string> FormatMessageLines(LogEventInfo logEvent)
        {
            var msg = Layout.Render(logEvent);
            return SplitNewlines ? msg.Split(_lineSeps, StringSplitOptions.RemoveEmptyEntries) : new[] { msg };
        }

        /// <summary>Calculates syslog PRIVAL</summary>
        /// <param name="facility">Syslog facility to transmit message from</param>
        /// <param name="severity">Syslog severity level</param>
        /// <returns>Byte array containing formatted syslog message</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculatePriorityValue(SyslogFacility facility, SyslogSeverity severity)
        {
            return (int)facility * 8 + (int)severity;
        }
    }
}