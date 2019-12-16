// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.MessageStorage;

namespace NLog.Targets.Syslog
{
    internal class LogEventMsgSet
    {
        private AsyncLogEventInfo asyncLogEvent;
        private readonly ByteArray buffer;
        private readonly MessageBuilder messageBuilder;
        private readonly MessageTransmitter messageTransmitter;
        private int currentMessage;
        private string[] logEntries;

        public LogEventMsgSet(AsyncLogEventInfo asyncLogEvent, ByteArray buffer, MessageBuilder messageBuilder, MessageTransmitter messageTransmitter)
        {
            this.asyncLogEvent = asyncLogEvent;
            this.buffer = buffer;
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
                return tcs.CanceledTask();

            var allSent = currentMessage == logEntries.Length;
            if (allSent)
                return tcs.SucceededTask(() => asyncLogEvent.Continuation(null));

            try
            {
                PrepareMessage();

                messageTransmitter
                    .SendMessageAsync(buffer, token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                        {
                            tcs.SetCanceled();
                            return;
                        }
                        if (t.Exception != null)
                        {
                            asyncLogEvent.Continuation(t.Exception.GetBaseException());
                            tcs.SetException(t.Exception);
                            return;
                        }
                        SendAsync(token, tcs);
                    }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                return tcs.Task;
            }
            catch (Exception exception)
            {
                return tcs.FailedTask(exception);
            }
        }

        private void PrepareMessage()
        {
            messageBuilder.PrepareMessage(buffer, asyncLogEvent.LogEvent, logEntries[currentMessage++]);
        }

        public override string ToString()
        {
            return asyncLogEvent.ToFormattedMessage();
        }
    }
}