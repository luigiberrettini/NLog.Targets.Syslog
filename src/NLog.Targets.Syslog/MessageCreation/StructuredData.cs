using NLog.Config;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageCreation
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

        internal void Initialize(Enforcement enforcement)
        {
            SdElements.ForEach(sdElem => sdElem.Initialize(enforcement));
        }

        internal IEnumerable<byte> Bytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            return SdElements.Count == 0 ? NilValueBytes : SdElement.Bytes(SdElements, logEvent, encodings);
        }
    }
}