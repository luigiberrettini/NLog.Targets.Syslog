namespace NLog.Targets
{
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
}