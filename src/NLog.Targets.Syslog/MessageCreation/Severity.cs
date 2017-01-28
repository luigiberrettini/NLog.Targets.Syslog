// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Severity
    {
        private readonly int value;
        private static readonly Severity Emergency = new Severity(0);
        //private static readonly Severity Alert = new Severity(1);
        //private static readonly Severity Critical = new Severity(2);
        private static readonly Severity Error = new Severity(3);
        private static readonly Severity Warning = new Severity(4);
        private static readonly Severity Notice = new Severity(5);
        private static readonly Severity Informational = new Severity(6);
        private static readonly Severity Debug = new Severity(7);
        private static readonly Dictionary<LogLevel, Severity> LogLevelToSeverity;

        static Severity()
        {
            LogLevelToSeverity = new Dictionary<LogLevel, Severity>
            {
                { LogLevel.Fatal, Emergency },
                { LogLevel.Error, Error },
                { LogLevel.Warn, Warning },
                { LogLevel.Info, Informational },
                { LogLevel.Debug, Debug },
                { LogLevel.Trace, Notice }
            };
        }

        private Severity(int value)
        {
            this.value = value;
        }

        public static explicit operator int(Severity severity)
        {
            return severity.value;
        }

        public static explicit operator Severity(LogLevel logLevel)
        {
            return LogLevelToSeverity[logLevel];
        }
    }
}