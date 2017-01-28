// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Settings;

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

        public void AppendBytes(ByteArray message, LogEventInfo logEvent, EncodingSet encodings)
        {
            var sdFromEvtProps = fromEventProperties.Render(logEvent);

            if (!string.IsNullOrEmpty(sdFromEvtProps))
            {
                var sdBytes = encodings.Utf8.GetBytes(sdFromEvtProps);
                message.Append(sdBytes);
                return;
            }

            if (sdElements.Count == 0)
                message.Append(NilValueBytes);
            else
                SdElement.AppendBytes(message, sdElements, logEvent, encodings);
        }

        public override string ToString()
        {
            return sdElements.Count == 0 ? NilValue : SdElement.ToString(sdElements);
        }
    }
}