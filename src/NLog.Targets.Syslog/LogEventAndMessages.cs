using System.Collections.Generic;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageCreation;

namespace NLog.Targets.Syslog
{
    internal class LogEventAndMessages
    {
        public AsyncLogEventInfo AsyncLogEvent { get; private set; }

        public Queue<IEnumerable<byte>> Messages { get; private set; }

        public LogEventAndMessages(MessageBuildersFacade messageBuilder, AsyncLogEventInfo asyncLogEvent, Layout layout)
        {
            var messages = messageBuilder.BuildMessages(asyncLogEvent.LogEvent, layout);

            AsyncLogEvent = asyncLogEvent;
            Messages = new Queue<IEnumerable<byte>>(messages);
        }
    }
}