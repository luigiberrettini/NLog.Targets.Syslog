// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog severities</summary>
    public enum Severity
    {
        /// <summary>Emergency severity</summary>
        Emergency = 0,

        /// <summary>Alert severity</summary>
        Alert = 1,

        /// <summary>Critical severity</summary>
        Critical = 2,

        /// <summary>Error severity</summary>
        Error = 3,

        /// <summary>Warning severity</summary>
        Warning = 4,

        /// <summary>Notice severity</summary>
        Notice = 5,

        /// <summary>Informational severity</summary>
        Informational = 6,

        /// <summary>Debug severity</summary>
        Debug = 7
    }
}