using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog.MessageSend
{
    public abstract class MessageTransmitter
    {
        private const string Localhost = "localhost";
        private const int DefaultPort = 514;
        protected const int BufferSize = 4096;

        /// <summary>The IP address of the Syslog server or an empty string</summary>
        protected string IpAddress { get; private set; }

        private string server;

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server
        {
            get { return server; }
            set { server = value; IpAddress = Dns.GetHostAddresses(Server).FirstOrDefault()?.ToString(); }
        }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port { get; set; }

        /// <summary>Builds the base part of a new instance of a class inheriting from MessageTransmitter</summary>
        protected MessageTransmitter()
        {
            Server = Localhost;
            Port = DefaultPort;
        }

        internal abstract void Initialize();

        internal virtual IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> message)
        {
            return message;
        }

        internal abstract Task SendMessageAsync(byte[] message, CancellationToken token);

        internal abstract void Dispose();
    }
}