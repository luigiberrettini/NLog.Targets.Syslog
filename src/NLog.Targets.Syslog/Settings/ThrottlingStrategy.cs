// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>The throttling strategy to be used</summary>
    public enum ThrottlingStrategy
    {
        None,
        DiscardOnFixedTimeout,
        DiscardOnPercentageTimeout,
        Discard,
        DeferForFixedTime,
        DeferForPercentageTime,
        Block
    }
}