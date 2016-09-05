using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;

namespace NLog.Targets.Syslog
{
    internal class LogEventMsgSet
    {
        private readonly AsyncLogEventInfo asyncLogEvent;
        private readonly MessageBuildersFacade messageBuilder;
        private readonly MessageTransmittersFacade messageTransmitter;
        private int currentMessage;
        private string[] logEntries;

        public LogEventMsgSet(AsyncLogEventInfo asyncLogEvent, MessageBuildersFacade messageBuilder, MessageTransmittersFacade messageTransmitter)
        {
            this.asyncLogEvent = asyncLogEvent;
            this.messageBuilder = messageBuilder;
            this.messageTransmitter = messageTransmitter;
            currentMessage = 0;
        }

        public LogEventMsgSet Build(Layout layout)
        {
            logEntries = messageBuilder.BuildLogEntries(asyncLogEvent.LogEvent, layout).ToArray();
            return this;
        }

        public Task SendAsync(CancellationToken token)
        {
            return SendAsync(token, new TaskCompletionSource<object>());
        }

        private Task SendAsync(CancellationToken token, TaskCompletionSource<object> tcs, Exception exception = null)
        {
            if (token.IsCancellationRequested)
                return CancelledTcsTask(tcs);

            // All messages have been dequeued and either all messages have been sent or
            // an exception has occurred and has been propagated till here
            // (no message was sent after it)
            if (HasNoMessages)
                return MessagesDequeuedTcsTask(tcs, exception);

            messageTransmitter
                .SendMessageAsync(NextMessage, token)
                .Then(t => SendAsync(token, tcs, t.Exception), token);

            return tcs.Task;
        }

        private bool HasNoMessages => currentMessage == logEntries.Length;

        private ByteArray NextMessage => messageBuilder.BuildMessage(asyncLogEvent.LogEvent, logEntries[currentMessage++]);

        private static Task CancelledTcsTask(TaskCompletionSource<object> tcs)
        {
            tcs.SetCanceled();
            return tcs.Task;
        }

        private Task MessagesDequeuedTcsTask(TaskCompletionSource<object> tcs, Exception exception)
        {
            InternalLogger.Debug($"Dequeued {asyncLogEvent.LogEvent.FormattedMessage}");

            if (exception != null)
            {
                asyncLogEvent.Continuation(exception.GetBaseException());
                tcs.SetException(exception);
            }
            else
            {
                asyncLogEvent.Continuation(null);
                tcs.SetResult(null);
            }

            return tcs.Task;
        }
    }
}