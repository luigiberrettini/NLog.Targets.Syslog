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
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public Enforcement Enforcement { get; set; }

        /// <summary>The builder used to create messages according to RFCs</summary>
        public MessageBuildersFacade MessageBuilder { get; set; }

        /// <summary>The transmitter used to send messages to the Syslog server</summary>
        public MessageTransmittersFacade MessageTransmitter { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            Enforcement = new Enforcement();
            MessageBuilder = new MessageBuildersFacade();
            MessageTransmitter = new MessageTransmittersFacade();
        }

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            MessageBuilder.Initialize(Enforcement);
            MessageTransmitter.Initialize();
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

        private IEnumerable<IEnumerable<byte>> ToMessages(AsyncLogEventInfo asyncLogEvent)
        {
            return MessageBuilder.BuildMessages(asyncLogEvent.LogEvent, Layout);
        }
    }
}