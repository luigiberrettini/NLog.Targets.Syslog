// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class AsyncLogEventInfoExtensions
    {
        public static string ToFormattedMessage(this AsyncLogEventInfo asyncLogEventInfo)
        {
            return asyncLogEventInfo.LogEvent.FormattedMessage;
        }
    }
}