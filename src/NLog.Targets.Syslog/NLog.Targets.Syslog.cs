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
    using System.Net;
    using System.Net.Sockets;
    using System.Globalization;
    using System.Net.Security;
    using System.Collections.Generic;
    using Layouts;
    using Common;

    /// <summary>
    /// This class enables logging to a unix-style syslog server using NLog.
    /// </summary>
    [Target("Syslog")]
    public class Syslog : TargetWithLayout
    {
        private const string NilValue = "-";
        private static readonly CultureInfo _usCulture = new CultureInfo("en-US");
        private static readonly byte[] _bom = { 0xEF, 0xBB, 0xBF };


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
        public Layout Sender { get; set; }

        /// <summary>
        /// Gets or sets the machine name hosting syslog
        /// </summary>
        public Layout MachineName { get; set; }

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
        /// RFC number for syslog protocol
        /// </summary>
        public RfcNumber Rfc { get; set; }

        #region RFC 5424 members

        /// <summary>
        /// Syslog protocol version for RFC 5424
        /// </summary>
        private byte ProtocolVersion { get; set; }

        /// <summary>
        /// Layout for PROCID protocol field
        /// </summary>
        public Layout ProcId { get; set; }

        /// <summary>
        /// Layout for MSGID protocol field
        /// </summary>
        public Layout MsgId { get; set; }

        /// <summary>
        /// Layout for STRUCTURED-DATA protocol field
        /// </summary>
        public Layout StructuredData { get; set; }

        #endregion

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
            this.MachineName = Dns.GetHostName();
            this.SplitNewlines = true;
            this.Rfc = RfcNumber.Rfc3164;

            //Defaults for rfc 5424
            this.ProtocolVersion = 1;
            this.ProcId = NilValue;
            this.MsgId = NilValue;
            this.StructuredData = NilValue;
        }

        /// <summary>
        /// Writes single event.
        /// No need to override sync version of Write(LogEventInfo) because it is called only from async version.
        /// </summary>
        /// <param name="logEvent">The NLog.AsyncLogEventInfo</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            SendEventsBatch(new[] { logEvent });
        }

        /// <summary>
        /// Writes array of events
        /// </summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            SendEventsBatch(logEvents);
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Elapsed {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Sends array of events to syslog server
        /// </summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        private void SendEventsBatch(AsyncLogEventInfo[] logEvents)
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

        /// <summary>
        /// Processes array of events and sends messages bytes using
        /// </summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <param name="messageSendAction">Implementation of send data method</param>
        void ProcessAndSendEvents(AsyncLogEventInfo[] logEvents, Action<byte[]> messageSendAction)
        {
            foreach (var asyncLogEvent in logEvents)
            {
                var logEvent = asyncLogEvent.LogEvent;
                var formattedMessageLines = this.GetFormattedMessageLines(logEvent);
                var severity = GetSyslogSeverity(logEvent.Level);
                foreach (var formattedMessageLine in formattedMessageLines)
                {
                    var message = this.BuildSyslogMessage(logEvent, this.Facility, severity, formattedMessageLine);
                    messageSendAction(message);
                }
            }
        }

        private IEnumerable<string> GetFormattedMessageLines(LogEventInfo logEvent)
        {
            var msg = this.Layout.Render(logEvent);
            return this.SplitNewlines ? msg.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) : new[] { msg };
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
                    return this.BuildSyslogMessage5424(logEvent, facility, priority, body);
                default:
                    return this.BuildSyslogMessage3164(logEvent, facility, priority, body);
            }
        }

        /// <summary>
        /// Builds rfc-3164 compatible message
        /// </summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="facility">Syslog Facility to transmit message from/param>
        /// <param name="priority">Syslog severity level</param>
        /// <param name="body">Message text/param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage3164(LogEventInfo logEvent, SyslogFacility facility, SyslogSeverity priority, string body)
        {
            // Calculate PRI field
            var calculatedPriority = (int)facility * 8 + (int)priority;
            var pri = "<" + calculatedPriority.ToString(CultureInfo.InvariantCulture) + ">";

            var time = logEvent.TimeStamp.ToLocalTime().ToString("MMM dd HH:mm:ss ", _usCulture);

            // Get sender machine name
            var machine = this.MachineName.Render(logEvent) + " ";

            var sender = this.Sender.Render(logEvent) + ": ";

            string[] strParams = { pri, time, machine, sender, body, Environment.NewLine };
            return Encoding.ASCII.GetBytes(string.Concat(strParams));
        }

        /// <summary>
        /// Builds rfc-5424 compatible message
        /// </summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="facility">Syslog Facility to transmit message from/param>
        /// <param name="priority">Syslog severity level</param>
        /// <param name="body">Message text/param>
        /// <returns>Byte array containing formatted syslog message</returns>
        private byte[] BuildSyslogMessage5424(LogEventInfo logEvent, SyslogFacility facility, SyslogSeverity priority, string body)
        {
            // Calculate PRI field
            var calculatedPriority = (int)facility * 8 + (int)priority;
            var pri = "<" + calculatedPriority.ToString(CultureInfo.InvariantCulture) + ">";
            var version = this.ProtocolVersion.ToString(CultureInfo.InvariantCulture);
            var time = logEvent.TimeStamp.ToString("o");
            // Get sender machine name
            var machine = this.MachineName.Render(logEvent);
            if (machine.Length > 255)
            {
                machine = machine.Substring(0, 255);
            }
            var sender = this.Sender.Render(logEvent);
            if (sender.Length > 48)
            {
                sender = sender.Substring(0, 48);
            }
            var procId = this.ProcId.Render(logEvent);
            if (procId.Length > 128)
            {
                procId = procId.Substring(0, 128);
            }
            var msgId = this.MsgId.Render(logEvent);
            if (msgId.Length > 32)
            {
                msgId = msgId.Substring(0, 32);
            }

            var headerData = Encoding.ASCII.GetBytes(string.Concat(pri, version, " ", time, " ", machine, " ", sender, " ", procId, " ", msgId, " "));
            var structuredData = Encoding.UTF8.GetBytes(this.StructuredData.Render(logEvent) + " ");
            var messageData = Encoding.UTF8.GetBytes(body);

            var allData = new List<byte>();
            allData.AddRange(headerData);
            allData.AddRange(structuredData);
            allData.AddRange(_bom);
            allData.AddRange(messageData);

            return allData.ToArray();
        }
    }
}
