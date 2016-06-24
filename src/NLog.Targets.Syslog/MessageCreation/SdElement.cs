using NLog.Config;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog SD-ELEMENT field</summary>
    [NLogConfigurationItem]
    public class SdElement
    {
        private SdIdToInvalidParamNamePattern sdIdToInvalidParamNamePattern;
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
            sdIdToInvalidParamNamePattern = new SdIdToInvalidParamNamePattern();
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

        public override string ToString()
        {
            return $"[{SdId}{SdParam.ToString(SdParams)}]";
        }

        internal static IEnumerable<byte> Bytes(IEnumerable<SdElement> sdElements, LogEventInfo logEvent, EncodingSet encodings)
        {
            var elements = sdElements.ToList();

            var ids = elements.Select(x => x.SdId);
            var encodedIds = SdId.Bytes(ids, logEvent, encodings).ToList();

            var encodedparamLists = elements
                .Select(x =>
                {
                    var renderedId = x.SdId.Render(logEvent);
                    var invalidParamNames = SdIdToInvalidParamNamePattern.Map(renderedId);
                    return SdParam.Bytes(x.SdParams, logEvent, invalidParamNames, encodings);
                })
                .ToList();

            return elements.SelectMany((e, i) => Bytes(encodedIds[i], encodedparamLists[i]));
        }

        private static IEnumerable<byte> Bytes(IEnumerable<byte> sdIdBytes, IEnumerable<byte> sdParamsBytes)
        {
            return LeftBracketBytes
                .Concat(sdIdBytes)
                .Concat(sdParamsBytes)
                .Concat(RightBracketBytes);
        }
    }
}