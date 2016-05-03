using NLog.Config;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
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

        /// <summary>Initializes the SdElement</summary>
        /// <param name="enforcement">The enforcement to apply</param>
        internal void Initialize(Enforcement enforcement)
        {
            SdId.Initialize(enforcement);
            SdParams.ForEach(sdParam => sdParam.Initialize(enforcement));
        }

        /// <summary>Gives the binary representation of a list of SD-ELEMENT fields</summary>
        /// <param name="sdElements">The SD-ELEMENT fields to be represented as binary</param>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="encodings">The encodings to be used</param>
        /// <returns>Bytes containing the list of SD-ELEMENT fields</returns>
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
                .Concat(LeftBracketBytes)
                .Concat(sdIdBytes)
                .Concat(sdParamsBytes)
                .Concat(RightBracketBytes);
        }
    }
}