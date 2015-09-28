namespace NLog.Targets
{
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
        /// informational messages
        /// </summary>
        Informational = 6,

        /// <summary>
        /// debug-level messages
        /// </summary>
        Debug = 7,
    }
}