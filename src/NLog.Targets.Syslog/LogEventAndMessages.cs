using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageCreation;

namespace NLog.Targets.Syslog
{
    // LogEventMsgSet
    internal class LogEventAndMessages
    {
        private readonly AsyncLogEventInfo asyncLogEventInfo;
        private int currentMessage;
        private IEnumerable<byte>[] messages;

        public bool HasNoMessages => currentMessage == messages.Length;

        public IEnumerable<byte> NextMessage => messages[currentMessage++];

        public LogEventAndMessages(AsyncLogEventInfo asyncLogEvent)
        {
            asyncLogEventInfo = asyncLogEvent;
            currentMessage = 0;
        }

        public void BuildMessages(MessageBuildersFacade messageBuilder, Layout layout)
        {
            messages = messageBuilder.BuildMessages(asyncLogEventInfo.LogEvent, layout).ToArray();
        }

        public void OnNoMessages(Exception exception) => asyncLogEventInfo.Continuation(exception);

        public void OnNoMessages() => asyncLogEventInfo.Continuation(null);

        public override string ToString() => asyncLogEventInfo.LogEvent.FormattedMessage;
    }
}