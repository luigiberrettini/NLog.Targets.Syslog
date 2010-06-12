using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace NLog.Targets
{
    [NLog.Targets.Target("Syslog")]
    public class Syslog : NLog.Targets.Target
    {
        public string SyslogServer { get; set; }
        public int Port { get; set; }
        public string Sender { get; set; }
        public SyslogFacility Facility { get; set; }

        #region Enumerations
        /// <summary>
		/// syslog severities
		/// </summary>
		/// <remarks>
		/// <para>
		/// The syslog severities.
		/// </para>
		/// </remarks>
		public enum SyslogSeverity
		{
			/// <summary>
			/// system is unusable
			/// </summary>
			Emergency = 0,

			/// <summary>
			/// action must be taken immediately
			/// </summary>
			Alert = 1,

			/// <summary>
			/// critical conditions
			/// </summary>
			Critical = 2,

			/// <summary>
			/// error conditions
			/// </summary>
			Error = 3,

			/// <summary>
			/// warning conditions
			/// </summary>
			Warning = 4,

			/// <summary>
			/// normal but significant condition
			/// </summary>
			Notice = 5,

			/// <summary>
			/// informational
			/// </summary>
			Informational = 6,

			/// <summary>
			/// debug-level messages
			/// </summary>
			Debug = 7
		};

		/// <summary>
		/// syslog facilities
		/// </summary>
		/// <remarks>
		/// <para>
		/// The syslog facilities
		/// </para>
		/// </remarks>
		public enum SyslogFacility
		{
			/// <summary>
			/// kernel messages
			/// </summary>
			Kernel = 0,

			/// <summary>
			/// random user-level messages
			/// </summary>
			User = 1,

			/// <summary>
			/// mail system
			/// </summary>
			Mail = 2,

			/// <summary>
			/// system daemons
			/// </summary>
			Daemons = 3,

			/// <summary>
			/// security/authorization messages
			/// </summary>
			Authorization = 4,

			/// <summary>
			/// messages generated internally by syslogd
			/// </summary>
			Syslog = 5,

			/// <summary>
			/// line printer subsystem
			/// </summary>
			Printer = 6,

			/// <summary>
			/// network news subsystem
			/// </summary>
			News = 7,

			/// <summary>
			/// UUCP subsystem
			/// </summary>
			Uucp = 8,

			/// <summary>
			/// clock (cron/at) daemon
			/// </summary>
			Clock = 9,

			/// <summary>
			/// security/authorization  messages (private)
			/// </summary>
			Authorization2 = 10,

			/// <summary>
			/// ftp daemon
			/// </summary>
			Ftp = 11,

			/// <summary>
			/// NTP subsystem
			/// </summary>
			Ntp = 12,

			/// <summary>
			/// log audit
			/// </summary>
			Audit = 13,

			/// <summary>
			/// log alert
			/// </summary>
			Alert = 14,

			/// <summary>
			/// clock daemon
			/// </summary>
			Clock2 = 15,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local0 = 16,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local1 = 17,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local2 = 18,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local3 = 19,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local4 = 20,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local5 = 21,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local6 = 22,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local7 = 23
		}

		#endregion Enumerations

        public Syslog()
        {
            // Sensible defaults...
            SyslogServer = "127.0.0.1";
            Port = 514;
            Sender = Assembly.GetCallingAssembly().GetName().Name;
            Facility = SyslogFacility.Local0;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            byte[] msg = buildSyslogMessage(Facility, getSyslogSeverity(logEvent.Level), DateTime.Now, Sender, logEvent.FormattedMessage);
            sendMessage(SyslogServer, Port, msg);
        }

        private void sendMessage(string SyslogServer, int Port, byte[] msg)
        {
            string ipAddress = Dns.GetHostEntry(SyslogServer).AddressList[0].ToString();
            UdpClient udp = new UdpClient(ipAddress, Port);
            udp.Send(msg, msg.Length);
            udp.Close();
            udp = null;
        }

        private SyslogSeverity getSyslogSeverity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Fatal)
                return SyslogSeverity.Emergency;
            else if (logLevel >= LogLevel.Error)
                return SyslogSeverity.Error;
            else if (logLevel >= LogLevel.Warn)
                return SyslogSeverity.Warning;
            else if (logLevel >= LogLevel.Info)
                return SyslogSeverity.Informational;
            else if (logLevel >= LogLevel.Debug)
                return SyslogSeverity.Debug;
            else if (logLevel >= LogLevel.Trace)
                return SyslogSeverity.Notice;
            else
                return SyslogSeverity.Notice;
        }

        private static byte[] buildSyslogMessage(SyslogFacility facilityLevel, SyslogSeverity priority, DateTime time, string sender, string body)
        {
            // Set the current Locale to "en-US" for proper date formatting
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            // Get sender machine name
            string machine = System.Net.Dns.GetHostName() + " ";

            // Calculate PRI field
            int calculatedPriority = (int)facilityLevel * 8 + (int)priority;
            string pri = "<" + calculatedPriority.ToString() + ">";

            string timeToString = time.ToString("MMM dd HH:mm:ss ");
            sender = sender + ": ";

            string[] strParams = { pri, timeToString, machine, sender, body };
            return Encoding.ASCII.GetBytes(string.Concat(strParams));
        }
    }
}
