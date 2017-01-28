// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog facilities</summary>
    public enum Facility
    {
        /// <summary>Kernel messages</summary>
        Kernel = 0,

        /// <summary>Random user-level messages</summary>
        User = 1,

        /// <summary>Mail system</summary>
        Mail = 2,

        /// <summary>System daemons</summary>
        Daemons = 3,

        /// <summary>Security/authorization messages</summary>
        Authorization = 4,

        /// <summary>Messages generated internally by syslogd</summary>
        Syslog = 5,

        /// <summary>Line printer subsystem</summary>
        Printer = 6,

        /// <summary>Network news subsystem</summary>
        News = 7,

        /// <summary>UUCP subsystem</summary>
        Uucp = 8,

        /// <summary>Clock (cron/at) daemon</summary>
        Clock = 9,

        /// <summary>Security/authorization messages (private)</summary>
        Authorization2 = 10,

        /// <summary>FTP daemon</summary>
        Ftp = 11,

        /// <summary>NTP subsystem</summary>
        Ntp = 12,

        /// <summary>Log audit</summary>
        Audit = 13,

        /// <summary>Log alert</summary>
        Alert = 14,

        /// <summary>Clock daemon</summary>
        Clock2 = 15,

        /// <summary>Reserved for local use</summary>
        Local0 = 16,

        /// <summary>Reserved for local use</summary>
        Local1 = 17,

        /// <summary>Reserved for local use</summary>
        Local2 = 18,

        /// <summary>Reserved for local use</summary>
        Local3 = 19,

        /// <summary>Reserved for local use</summary>
        Local4 = 20,

        /// <summary>Reserved for local use</summary>
        Local5 = 21,

        /// <summary>Reserved for local use</summary>
        Local6 = 22,

        /// <summary>Reserved for local use</summary>
        Local7 = 23
    }
}