// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class SdElement
    {
        private static readonly InternalLogDuplicatesPolicy LogDuplicatesPolicy = new InternalLogDuplicatesPolicy();
        private static readonly byte[] LeftBracketBytes = { 0x5B };
        private static readonly byte[] RightBracketBytes = { 0x5D };

        private readonly SdId sdId;
        private readonly List<SdParam> sdParams;

        public SdElement(SdElementConfig sdElementConfig, EnforcementConfig enforcementConfig)
        {
            sdId = new SdId(sdElementConfig.SdId, enforcementConfig);
            sdParams = sdElementConfig.SdParams.Select(sdParamConfig => new SdParam(sdParamConfig, enforcementConfig)).ToList();
        }

        public static void Append(ByteArray message, List<SdElement> sdElements, LogEventInfo logEvent)
        {
            if (LogDuplicatesPolicy.IsApplicable())
                LogDuplicatesPolicy.Apply(sdElements, x => x.sdId.Render(logEvent));

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
            return sdElements.Aggregate(string.Empty, (acc, curr) => acc + curr);
        }

        public override string ToString()
        {
            return $"[{sdId}{SdParam.ToString(sdParams)}]";
        }
    }
}