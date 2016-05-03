using NLog.Config;
using System.Collections.Generic;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>A Syslog STRUCTURED-DATA part</summary>
    [NLogConfigurationItem]
    public class StructuredData
    {
        private static readonly byte[] NilValueBytes = { 0x2D };

        /// <summary>The SD-ELEMENTs contained in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdElement), nameof(SdElement))]
        public IList<SdElement> SdElements { get; set; }

        /// <summary>Builds a new instance of the StructuredData class</summary>
        public StructuredData()
        {
            SdElements = new List<SdElement>();
        }

        /// <summary>Initializes the StructuredData</summary>
        /// <param name="enforcement">The enforcement to apply</param>
        internal void Initialize(Enforcement enforcement)
        {
            SdElements.ForEach(sdElem => sdElem.Initialize(enforcement));
        }

        /// <summary>Gives the binary representation of the STRUCTURED_DATA part</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="encodings">The encodings to be used</param>
        /// <returns>Bytes containing the STRUCTURED_DATA part</returns>
        internal IEnumerable<byte> Bytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            return SdElements.Count == 0 ? NilValueBytes : SdElement.Bytes(SdElements, logEvent, encodings);
        }
    }
}