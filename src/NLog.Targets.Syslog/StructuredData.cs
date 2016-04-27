using NLog.Config;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>Initializes a new instance of the StructuredData class</summary>
        public StructuredData()
        {
            SdElements = new List<SdElement>();
        }

        public IEnumerable<byte> Bytes(LogEventInfo logEvent)
        {
            return SdElements.Count == 0 ? NilValueBytes : SdElements.SelectMany(sdElement => sdElement.Bytes(logEvent));
        }
    }
}