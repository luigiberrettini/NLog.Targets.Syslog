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