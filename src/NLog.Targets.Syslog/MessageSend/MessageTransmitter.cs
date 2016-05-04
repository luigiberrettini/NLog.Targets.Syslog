using System.Collections.Generic;
using System.Linq;
using System.Net;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public abstract class MessageTransmitter
    {
        private const string Localhost = "localhost";
        private const int DefaultSyslogPort = 514;

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

        protected MessageTransmitter()
        {
            Server = Localhost;
            Port = DefaultSyslogPort;
        }

        /// <summary>Applies a protocol specific framing method, if supported, to a Syslog message</summary>
        /// <param name="syslogMessage">The message to be framed</param>
        /// <returns>Bytes containing the framed Syslog message or the original Syslog message</returns>
        public virtual IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return syslogMessage;
        }

        /// <summary>Sends a set of Syslog messages with a protocol and the related settings</summary>
        /// <param name="syslogMessages">The messages to be sent</param>
        public abstract void SendMessages(IEnumerable<byte[]> syslogMessages);
    }
}