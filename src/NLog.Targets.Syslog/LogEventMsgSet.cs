using System;
using System.Linq;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageCreation;

namespace NLog.Targets.Syslog
{
    internal class LogEventMsgSet
    {
        private readonly AsyncLogEventInfo asyncLogEvent;
        private readonly MessageBuildersFacade messageBuilder;
        private readonly Layout layout;
        private int currentMessage;
        private string[] logEntries;

        public bool HasNoMessages => currentMessage == logEntries.Length;

        public ByteArray NextMessage => messageBuilder.BuildMessage(asyncLogEvent.LogEvent, logEntries[currentMessage++]);

        public LogEventMsgSet(AsyncLogEventInfo asyncLogEvent, MessageBuildersFacade messageBuilder, Layout layout)
        {
            this.asyncLogEvent = asyncLogEvent;
            this.messageBuilder = messageBuilder;
            this.layout = layout;
            currentMessage = 0;
        }

        public void BuildLogEntries()
        {
            logEntries = messageBuilder.BuildLogEntries(asyncLogEvent.LogEvent, layout).ToArray();
        }

        public void OnNoMessages(Exception exception) => asyncLogEvent.Continuation(exception);

        public void OnNoMessages() => asyncLogEvent.Continuation(null);

        public override string ToString() => asyncLogEvent.LogEvent.FormattedMessage;
    }
}