using System;
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
        private string ipAddress;

        /// <summary>The IP address of the Syslog server or an empty string</summary>
        protected string IpAddress => ipAddress ?? (ipAddress = Dns.GetHostAddresses(Server).FirstOrDefault()?.ToString() ?? string.Empty);

        /// <summary>The IP address or hostname of the Syslog server</summary>
        public string Server { get; set; }

        /// <summary>The port number the Syslog server is listening on</summary>
        public int Port { get; set; }

        protected MessageTransmitter()
        {
            Server = Localhost;
            Port = DefaultSyslogPort;
        }

        /// <summary>Applies a protocol specific framing method, if supported, to a Syslog syslogMessage</summary>
        /// <param name="syslogMessage">The message to be framed</param>
        /// <returns>Bytes containing the framed Syslog message or the original Syslog syslogMessage</returns>
        public virtual IEnumerable<byte> FrameMessageOrLeaveItUnchanged(IEnumerable<byte> syslogMessage)
        {
            return syslogMessage;
        }

        /// <summary>Convert a MessageTransmitter to a protocol type</summary>
        /// <param name="messageTransmitter">MessageTransmitter to convert</param>
        /// <returns>The protocol type which corresponds to the MessageTransmitter</returns>
        public static explicit operator ProtocolType(MessageTransmitter messageTransmitter)
        {
            return (ProtocolType)Enum.Parse(typeof(ProtocolType), messageTransmitter.GetType().Name);
        }

        /// <summary>Sends a set of Syslog messages with a protocol and the related settings</summary>
        /// <param name="syslogMessages">The messages to be sent</param>
        public abstract void SendMessages(IEnumerable<byte[]> syslogMessages);
    }
}