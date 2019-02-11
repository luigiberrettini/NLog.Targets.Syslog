// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal abstract class MessageTransmitter
    {
        private static readonly Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>> TransmitterFactory;
        protected static readonly TimeSpan ZeroSeconds = TimeSpan.FromSeconds(0);

        private volatile bool neverCalledInit;
        private volatile bool isReady;
        private readonly TimeSpan newInitDelay;

        protected string Server { get; }

        protected string IpAddress => Dns.GetHostAddresses(Server).FirstOrDefault()?.ToString();

        protected int Port { get; }

        static MessageTransmitter()
        {
            TransmitterFactory = new Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>>
            {
                { ProtocolType.Udp, messageTransmitterConfig => new Udp(messageTransmitterConfig.Udp) },
                { ProtocolType.Tcp, messageTransmitterConfig => new Tcp(messageTransmitterConfig.Tcp) }
            };
        }

        public static MessageTransmitter FromConfig(MessageTransmitterConfig messageTransmitterConfig)
        {
            return TransmitterFactory[messageTransmitterConfig.Protocol](messageTransmitterConfig);
        }

        protected MessageTransmitter(string server, int port, int reconnectInterval)
        {
            neverCalledInit = true;
            isReady = false;
            newInitDelay = TimeSpan.FromMilliseconds(reconnectInterval);
            Server = server;
            Port = port;
        }

        public Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            return PrepareForSendAsync(token)
                .Then(_ => SendAsync(message, token), token)
                .Unwrap()
                .ContinueWith(t =>
                {
                    if (t.Exception == null) // t.IsFaulted is false
                        return Task.FromResult<object>(null);

                    InternalLogger.Warn(t.Exception?.GetBaseException(), "SendAsync failed");
                    TidyUp();
                    return SendMessageAsync(message, token); // Failures impact on the log entry queue
                }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                .Unwrap();
        }

        public void Dispose()
        {
            TidyUp();
        }

        protected abstract Task Init();

        protected abstract Task SendAsync(ByteArray message, CancellationToken token);

        protected abstract void Terminate();

        private Task PrepareForSendAsync(CancellationToken token)
        {
            if (isReady)
                return Task.FromResult<object>(null);

            var delay = neverCalledInit ? ZeroSeconds : newInitDelay;
            neverCalledInit = false;
            return Task
                .Delay(delay, token)
                .Then(_ => Init(), token)
                .Unwrap()
                .Then(_ => isReady = true, token);
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
                InternalLogger.Warn(exception, "Terminate failed");
            }
            finally
            {
                isReady = false;
            }
        }
    }
}