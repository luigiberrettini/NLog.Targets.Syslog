// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class SdElement
    {
        private static readonly byte[] LeftBracketBytes = { 0x5B };
        private static readonly byte[] RightBracketBytes = { 0x5D };

        private readonly SdId sdId;
        private readonly IList<SdParam> sdParams;

        public SdElement(SdElementConfig sdElementConfig, EnforcementConfig enforcementConfig)
        {
            sdId = new SdId(sdElementConfig.SdId, enforcementConfig);
            sdParams = sdElementConfig.SdParams.Select(sdParamConfig => new SdParam(sdParamConfig, enforcementConfig)).ToList();
        }

        public static void Append(ByteArray message, IList<SdElement> sdElements, LogEventInfo logEvent)
        {
            foreach (var sdElement in sdElements)
            {
                var renderedSdId = sdElement.sdId.Render(logEvent);
                message.AppendBytes(LeftBracketBytes);
                sdElement.sdId.Append(message, renderedSdId);
                SdParam.Append(message, sdElement.sdParams, logEvent, SdIdToInvalidParamNamePattern.Map(renderedSdId));
                message.AppendBytes(RightBracketBytes);
            }
        }

        public static string ToString(IEnumerable<SdElement> sdElements)
        {
            return sdElements.Aggregate(string.Empty, (acc, curr) => acc.ToString() + curr.ToString());
        }

        public override string ToString()
        {
            return $"[{sdId}{SdParam.ToString(sdParams)}]";
        }
    }
}