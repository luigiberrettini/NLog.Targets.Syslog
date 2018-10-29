// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class LogLevelSeverityMapping
    {
        private readonly IDictionary<LogLevel, Severity> logLevelSeverityMapping;

        public LogLevelSeverityMapping(LogLevelSeverityConfig logLevelSeverityConfig)
        {
            logLevelSeverityMapping = new Dictionary<LogLevel, Severity>
            {
                { LogLevel.Fatal, logLevelSeverityConfig.Fatal },
                { LogLevel.Error, logLevelSeverityConfig.Error },
                { LogLevel.Warn, logLevelSeverityConfig.Warn },
                { LogLevel.Info, logLevelSeverityConfig.Info },
                { LogLevel.Debug, logLevelSeverityConfig.Debug },
                { LogLevel.Trace, logLevelSeverityConfig.Trace }
            };
        }

        public Severity this[LogLevel logLevel] =>  logLevelSeverityMapping[logLevel];
    }
}