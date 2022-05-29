// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;
using ProtocolType = NLog.Targets.Syslog.Settings.ProtocolType;

namespace NLog.Targets.Syslog.MessageSend
{
    internal abstract class MessageTransmitter
    {
        private static readonly Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>> TransmitterFactory;

        private volatile bool isReady;
        private readonly Predicate<int> canRetry;
        private readonly BackoffDelayProvider delayProvider;

        protected string Server { get; }

        protected int Port { get; }

        static MessageTransmitter()
        {
            TransmitterFactory = new Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>>
            {
                { ProtocolType.Udp, messageTransmitterConfig => new Udp(messageTransmitterConfig.Udp, messageTransmitterConfig.Retry) },
                { ProtocolType.Tcp, messageTransmitterConfig => new Tcp(messageTransmitterConfig.Tcp, messageTransmitterConfig.Retry) }
            };
        }

        public static MessageTransmitter FromConfig(MessageTransmitterConfig messageTransmitterConfig)
        {
            return TransmitterFactory[messageTransmitterConfig.Protocol](messageTransmitterConfig);
        }

        protected MessageTransmitter(Layout server, int port, RetryConfig retryConfig)
        {
            isReady = false;
            canRetry = retryNumber => retryConfig.InfiniteRetries || retryNumber < retryConfig.Max;
            delayProvider = BackoffDelayProvider.FromConfig(retryConfig);
            Server = server?.Render(LogEventInfo.CreateNullEvent());
            Port = port;
        }

        public Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            return SendMessageAsync(message, 0, token);
        }

        public void Dispose()
        {
            TidyUp();
        }

        protected abstract Task Init(IPEndPoint ipEndPoint);

        protected abstract Task SendAsync(ByteArray message, CancellationToken token);

        protected abstract void Terminate();

        private Task SendMessageAsync(ByteArray message, int retryNumber, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            return PrepareForSendAsync(retryNumber, token)
                .Then(_ => SendAsync(message, token), token)
                .ContinueWith(t =>
                {
                    var baseException = t.Exception?.GetBaseException();

                    // Complete with success: cancellation requested or message sent
                    if (token.IsCancellationRequested || t.IsCanceled || baseException == null)
                        return Task.FromResult<object>(null);

                    InternalLogger.Warn(baseException, "[Syslog] SendAsync failed");
                    TidyUp();

                    // Retry: failures can impact on the log entry queue
                    if (canRetry(retryNumber))
                        return SendMessageAsync(message, retryNumber < int.MaxValue ? ++retryNumber : retryNumber, token);

                    // Complete with failure: message not sent and lost
                    var tcs = new TaskCompletionSource<object>();
                    tcs.SetException(baseException);
                    return tcs.Task;
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                .Unwrap();
        }

        private Task PrepareForSendAsync(int retryNumber, CancellationToken token)
        {
            if (isReady)
                return Task.FromResult<object>(null);

            var delay = retryNumber == 0 ? TimeSpan.Zero : delayProvider.GetDelay(retryNumber == 1);
            return Task
                .Delay(delay, token)
                .Then(_ => Init(GetIpEndPoint()), token)
                .Then(_ => isReady = true, token);
        }

        private IPEndPoint GetIpEndPoint()
        {
            return Dns
                .GetHostAddresses(Server)
                .Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4 ||
                                    ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6)
                .OrderBy(x => x.AddressFamily)
                .Select(x => new IPEndPoint(x, Port))
                .FirstOrDefault();
        }

        private void TidyUp()
        {
            try
            {
                if (isReady)
                    Terminate();
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "[Syslog] Terminate failed");
            }
            finally
            {
                isReady = false;
            }
        }
    }
}