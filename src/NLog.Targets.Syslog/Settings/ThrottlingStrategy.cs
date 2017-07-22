// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>The throttling strategy to be used</summary>
    public enum ThrottlingStrategy
    {
        /// <summary>No throttling strategy</summary>
        None,

        /// <summary>Discard after a fixed timeout</summary>
        DiscardOnFixedTimeout,

        /// <summary>Discard after a timeout percentage of the log entries waiting to be processed</summary>
        DiscardOnPercentageTimeout,

        /// <summary>Discard log entries</summary>
        Discard,

        /// <summary>Defer for a fixed time</summary>
        DeferForFixedTime,

        /// <summary>Defer for a time percentage of the log entries waiting to be processed</summary>
        DeferForPercentageTime,

        /// <summary>Block indefinitely until waiting log entries decrease</summary>
        Block
    }
}