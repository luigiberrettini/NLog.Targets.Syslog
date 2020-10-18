// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class StructuredData
    {
        private const string NilValue = "-";
        private static readonly byte[] NilValueBytes = { 0x2D };

        private readonly Layout fromEventProperties;
        private readonly IList<SdElement> sdElements;

        public StructuredData(StructuredDataConfig sdConfig, EnforcementConfig enforcementConfig)
        {
            fromEventProperties = sdConfig.FromEventProperties;
            sdElements = sdConfig.SdElements.Select(sdElementConfig => new SdElement(sdElementConfig, enforcementConfig)).ToList();
        }

        public void Append(ByteArray message, LogEventInfo logEvent)
        {
            var sdFromEvtProps = fromEventProperties.Render(logEvent);

            if (!string.IsNullOrEmpty(sdFromEvtProps))
            {
                message.AppendUtf8(sdFromEvtProps);
                return;
            }

            if (sdElements.Count == 0)
                message.AppendBytes(NilValueBytes);
            else
                SdElement.Append(message, sdElements, logEvent);
        }

        public override string ToString()
        {
            return sdElements.Count == 0 ? NilValue : SdElement.ToString(sdElements);
        }
    }
}