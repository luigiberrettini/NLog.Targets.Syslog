// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc cref="NotifyPropertyChanged" />
    /// <summary>Log level to severity configuration</summary>
    public class LogLevelSeverityConfig : NotifyPropertyChanged
    {
        private Severity fatal;
        private Severity error;
        private Severity warn;
        private Severity info;
        private Severity debug;
        private Severity trace;

        /// <summary>The Syslog severity for log level fatal</summary>
        public Severity Fatal
        {
            get => fatal;
            set => SetProperty(ref fatal, value);
        }

        /// <summary>The Syslog severity for log level error</summary>
        public Severity Error
        {
            get => error;
            set => SetProperty(ref error, value);
        }

        /// <summary>The Syslog severity for log level warn</summary>
        public Severity Warn
        {
            get => warn;
            set => SetProperty(ref warn, value);
        }

        /// <summary>The Syslog severity for log level info</summary>
        public Severity Info
        {
            get => info;
            set => SetProperty(ref info, value);
        }

        /// <summary>The Syslog severity for log level debug</summary>
        public Severity Debug
        {
            get => debug;
            set => SetProperty(ref debug, value);
        }

        /// <summary>The Syslog severity for log level trace</summary>
        public Severity Trace
        {
            get => trace;
            set => SetProperty(ref trace, value);
        }

        /// <summary>Builds a new instance of the LogLevelSeverityConfig class</summary>
        public LogLevelSeverityConfig()
        {
            fatal = Severity.Emergency;
            error = Severity.Error;
            warn = Severity.Warning;
            info = Severity.Informational;
            debug = Severity.Debug;
            trace = Severity.Notice;
        }
    }
}