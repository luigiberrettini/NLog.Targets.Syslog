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
            logEntries = messageBuilder.BuildLogEntries(asyncLogEvent.LogEvent, layout);
            return this;
        }

        public Task SendAsync(CancellationToken token)
        {
            return SendAsync(token, new TaskCompletionSource<object>());
        }

        private Task SendAsync(CancellationToken token, TaskCompletionSource<object> tcs)
        {
            if (token.IsCancellationRequested)
                return SendCanceledTcsTask(tcs);

            if (AllSent)
                return SendSucceededTcsTask(tcs);

            messageTransmitter
                .SendMessageAsync(NextMessage, token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        return SendCanceledTcsTask(tcs);
                    if (t.Exception != null)
                        return SendFailedTcsTask(tcs, t.Exception);
                    return SendAsync(token, tcs);
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                .Unwrap();

            return tcs.Task;
        }

        private bool AllSent => currentMessage == logEntries.Length;

        private ByteArray NextMessage => messageBuilder.BuildMessage(asyncLogEvent.LogEvent, logEntries[currentMessage++]);

        private static Task SendCanceledTcsTask(TaskCompletionSource<object> tcs)
        {
            tcs.SetCanceled();
            return tcs.Task;
        }

        private Task SendSucceededTcsTask(TaskCompletionSource<object> tcs)
        {
            asyncLogEvent.Continuation(null);
            tcs.SetResult(null);
            return tcs.Task;
        }

        private Task SendFailedTcsTask(TaskCompletionSource<object> tcs, Exception exception)
        {
            asyncLogEvent.Continuation(exception.GetBaseException());
            tcs.SetException(exception);
            return tcs.Task;
        }

        public override string ToString()
        {
            return asyncLogEvent.LogEvent.FormattedMessage;
        }
    }
}