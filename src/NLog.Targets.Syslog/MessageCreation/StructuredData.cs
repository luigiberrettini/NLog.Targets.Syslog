using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog STRUCTURED-DATA part</summary>
    [NLogConfigurationItem]
    public class StructuredData
    {
        private const string NilValue = "-";
        private static readonly byte[] NilValueBytes = { 0x2D };

        public Layout FromEventProperties { get; set; }

        /// <summary>The SD-ELEMENTs contained in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdElement), nameof(SdElement))]
        public IList<SdElement> SdElements { get; set; }

        /// <summary>Builds a new instance of the StructuredData class</summary>
        public StructuredData()
        {
            FromEventProperties = string.Empty;
            SdElements = new List<SdElement>();
        }

        internal void Initialize(Enforcement enforcement)
        {
            SdElements.ForEach(sdElem => sdElem.Initialize(enforcement));
        }

        public override string ToString()
        {
            return SdElements.Count == 0 ? NilValue : SdElement.ToString(SdElements);
        }

        internal IEnumerable<byte> Bytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            var sdFromEvtProps = FromEventProperties.Render(logEvent);
            if (!string.IsNullOrEmpty(sdFromEvtProps))
                return encodings.Utf8.GetBytes(sdFromEvtProps);

            return SdElements.Count == 0 ? NilValueBytes : SdElement.Bytes(SdElements, logEvent, encodings);
        }
    }
}