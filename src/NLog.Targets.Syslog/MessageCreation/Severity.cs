using System;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Syslog severities</summary>
    internal class Severity : IComparable<Severity>
    {
        private readonly int value;
        private readonly string displayName;

        /// <summary>System is unusable</summary>
        public static readonly Severity Emergency = new Severity(0, "Emergency");

        /// <summary>Action must be taken immediately</summary>
        public static readonly Severity Alert = new Severity(1, "Alert");

        /// <summary>Critical conditions</summary>
        public static readonly Severity Critical = new Severity(2, "Critical");

        /// <summary>Error conditions</summary>
        public static readonly Severity Error = new Severity(3, "Error");

        /// <summary>Warning conditions</summary>
        public static readonly Severity Warning = new Severity(4, "Warning");

        /// <summary>Normal but significant condition</summary>
        public static readonly Severity Notice = new Severity(5, "Notice");

        /// <summary>Informational messages</summary>
        public static readonly Severity Informational = new Severity(6, "Informational");

        /// <summary>Debug-level messages</summary>
        public static readonly Severity Debug = new Severity(7, "Debug");

        private Severity(int value, string displayName)
        {
            this.value = value;
            this.displayName = displayName;
        }

        /// <summary>Compare this instance of SyslogSeverity to another</summary>
        /// <param name="other">The instance of SyslogSeverity this instance is to be compared with</param>
        public int CompareTo(Severity other)
        {
            return ((int)this).CompareTo((int)other);
        }

        /// <summary>Convert a Syslog severity to an integer</summary>
        /// <param name="severity">Syslog severity to convert</param>
        /// <returns>SyslogSeverity which corresponds to the NLog level</returns>
        public static explicit operator int(Severity severity)
        {
            return severity.value;
        }

        /// <summary>Convert an NLog level to a Syslog severity as they are not exactly one to one</summary>
        /// <param name="logLevel">NLog log level to convert</param>
        /// <returns>SyslogSeverity which corresponds to the NLog level</returns>
        public static explicit operator Severity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Fatal)
                return Emergency;

            if (logLevel == LogLevel.Error)
                return Error;

            if (logLevel == LogLevel.Warn)
                return Warning;

            if (logLevel == LogLevel.Info)
                return Informational;

            if (logLevel == LogLevel.Debug)
                return Debug;

            if (logLevel == LogLevel.Trace)
                return Notice;

            throw new InvalidOperationException($"Unsupported log level {logLevel}");
        }

        /// <summary>Convert a Syslog severity to a string</summary>
        /// <returns>The name of the Syslog severity</returns>
        public override string ToString()
        {
            return displayName;
        }
    }
}