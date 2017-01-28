// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Settings;

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
            sdId = new SdId(sdElementConfig.SdId , enforcementConfig);
            sdParams = sdElementConfig.SdParams.Select(sdParamConfig => new SdParam(sdParamConfig, enforcementConfig)).ToList();
        }

        public static void AppendBytes(ByteArray message, IEnumerable<SdElement> sdElements, LogEventInfo logEvent, EncodingSet encodings)
        {
            var elements = sdElements
                .Select(x => new { SdId = x.sdId, RenderedSdId = x.sdId.Render(logEvent), SdParams = x.sdParams })
                .ToList();

            InternalLogDuplicatesPolicy.Apply(elements, x => x.RenderedSdId);

            elements
                .ForEach(elem =>
                {
                    message.Append(LeftBracketBytes);
                    elem.SdId.AppendBytes(message, elem.RenderedSdId, encodings);
                    SdParam.AppendBytes(message, elem.SdParams, logEvent, SdIdToInvalidParamNamePattern.Map(elem.RenderedSdId), encodings);
                    message.Append(RightBracketBytes);
                });
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