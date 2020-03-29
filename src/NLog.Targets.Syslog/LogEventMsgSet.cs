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
        private AsyncLogEventInfo asyncLogEventInfo;
        private readonly ByteArray buffer;
        private readonly MessageBuilder messageBuilder;
        private readonly MessageTransmitter messageTransmitter;
        private string firstLogEntry;
        private int currentMessage;
        private string[] splitLogEntries;

        public LogEventMsgSet(AsyncLogEventInfo asyncLogEventInfo, ByteArray buffer, MessageBuilder messageBuilder, MessageTransmitter messageTransmitter)
        {
            this.asyncLogEventInfo = asyncLogEventInfo;
            this.buffer = buffer;
            this.messageBuilder = messageBuilder;
            this.messageTransmitter = messageTransmitter;
            currentMessage = 0;
        }

        public LogEventMsgSet Build(Layout layout)
        {
            firstLogEntry = messageBuilder.BuildLogEntry(asyncLogEventInfo.LogEvent, layout, out splitLogEntries);
            return this;
        }

        public Task SendAsync(CancellationToken token)
        {
            return SendAsync(token, new TaskCompletionSource<object>());
        }

        private Task SendAsync(CancellationToken token, TaskCompletionSource<object> tcs)
        {
            if (token.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            var allSent = firstLogEntry == null || currentMessage == (splitLogEntries?.Length ?? 1);
            if (allSent)
            {
                asyncLogEventInfo.Continuation(null);
                tcs.SetResult(null);
                return tcs.Task;
            }

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
                            asyncLogEventInfo.Continuation(t.Exception.GetBaseException());
                            tcs.SetException(t.Exception);
                            return;
                        }
                        SendAsync(token, tcs);
                    }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                return tcs.Task;
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
                return tcs.Task;
            }
        }

        private void PrepareMessage()
        {
            var logEntry = ++currentMessage == 1 ? firstLogEntry : splitLogEntries[currentMessage];
            messageBuilder.PrepareMessage(buffer, asyncLogEventInfo.LogEvent, logEntry);
        }

        public override string ToString()
        {
            return asyncLogEventInfo.ToFormattedMessage();
        }
    }
}