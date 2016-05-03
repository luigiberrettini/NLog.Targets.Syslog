////////////////////////////////////////////////////////////////////////////////
//   NLog.Targets.Syslog
//   ------------------------------------------------------------------------
//   Copyright 2013 Jesper Hess Nielsen <jesper@graffen.dk>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
////////////////////////////////////////////////////////////////////////////////

using NLog.Common;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        /// <summary>The transmitter used to send messages to the Syslog server</summary>
        public MessageTransmittersFacade MessageTransmitter { get; set; }

        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public Enforcement Enforcement { get; set; }

        /// <summary>The builder used to create messages according to RFCs</summary>
        public MessageBuildersFacade MessageBuilder { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            MessageTransmitter = new MessageTransmittersFacade();
            Enforcement = new Enforcement();
            MessageBuilder = new MessageBuildersFacade();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            MessageTransmitter.Initialize();
            MessageBuilder.Initialize(Enforcement);
        }

        /// <summary>Writes a single event</summary>
        /// <param name="logEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to ovveride it</remarks>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            SendMessages(logEvent);
        }

        /// <summary>Writes array of events</summary>
        /// <param name="logEvents">The array of NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to ovveride it</remarks>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            SendMessages(logEvents);
        }

        private void SendMessages(params AsyncLogEventInfo[] asyncLogEvents)
        {
            var messages = asyncLogEvents.SelectMany(ToMessages);
            MessageTransmitter.SendMessages(messages);
        }

        private IEnumerable<byte[]> ToMessages(AsyncLogEventInfo asyncLogEvent)
        {
            var messages = MessageBuilder
                .BuildMessages(asyncLogEvent.LogEvent, Layout)
                .Select(syslogMessage => MessageTransmitter.FrameMessageOrLeaveItUnchanged(syslogMessage).ToArray());
            return messages;
        }
    }
}