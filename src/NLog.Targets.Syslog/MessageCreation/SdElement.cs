using NLog.Config;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog SD-ELEMENT field</summary>
    [NLogConfigurationItem]
    public class SdElement
    {
        private static readonly byte[] LeftBracketBytes = { 0x5B };
        private static readonly byte[] RightBracketBytes = { 0x5D };

        /// <summary>The SD-ID field of an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        public SdId SdId { get; set; }

        /// <summary>The SD-PARAM fields belonging to an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdParam), nameof(SdParam))]
        public IList<SdParam> SdParams { get; set; }

        /// <summary>Builds a new instance of the SdElement class</summary>
        public SdElement()
        {
            SdParams = new List<SdParam>();
        }

        internal void Initialize(Enforcement enforcement)
        {
            SdId.Initialize(enforcement);
            SdParams.ForEach(sdParam => sdParam.Initialize(enforcement));
        }

        internal static string ToString(IEnumerable<SdElement> sdElements)
        {
            return sdElements.Aggregate(string.Empty, (acc, curr) => acc.ToString() + curr.ToString());
        }

        /// <summary>Gives a string representation of an SdElement instance</summary>
        public override string ToString()
        {
            return $"[{SdId}{SdParam.ToString(SdParams)}]";
        }

        internal static void AppendBytes(ByteArray message, IEnumerable<SdElement> sdElements, LogEventInfo logEvent, EncodingSet encodings)
        {
            var elements = sdElements
                .Select(x => new { x.SdId, RenderedSdId = x.SdId.Render(logEvent), x.SdParams })
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
    }
}