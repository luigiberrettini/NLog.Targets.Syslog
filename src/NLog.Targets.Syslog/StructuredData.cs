using NLog.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private const string NilValue = "-";

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
            return SdElements.Count == 0 ?
                Encoding.ASCII.GetBytes(NilValue) :
                SdElements.SelectMany(sdElement => sdElement.Bytes(logEvent));
        }
    }
}