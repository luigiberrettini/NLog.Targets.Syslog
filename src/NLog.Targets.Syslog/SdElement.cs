using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private static readonly byte[] LeftBracketBytes = Encoding.ASCII.GetBytes("[");
        private static readonly byte[] SpaceBytes = Encoding.ASCII.GetBytes(" ");
        private static readonly byte[] RightBracketBytes = Encoding.ASCII.GetBytes("]");

        /// <summary>The SD-ID field of an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        public Layout SdId { get; set; }

        /// <summary>The SD-PARAM fields belonging to an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdParam), nameof(SdParam))]
        public IList<SdParam> SdParams { get; set; }

        /// <summary>Initializes a new instance of the SdElement class</summary>
        public SdElement()
        {
            SdParams = new List<SdParam>();
        }

        /// <summary>Gives the binary representation of this SD-ELEMENT field</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <returns>Bytes containing this SD-ELEMENT field</returns>
        public IEnumerable<byte> Bytes(LogEventInfo logEvent)
        {
            return LeftBracketBytes
                .Concat(LeftBracketBytes)
                .Concat(SdIdBytes(logEvent))
                .Concat(SdParamsBytes(logEvent))
                .Concat(RightBracketBytes);
        }

        private IEnumerable<byte> SdIdBytes(LogEventInfo logEvent)
        {
            var sdId = SdId.Render(logEvent);
            return Encoding.ASCII.GetBytes(sdId);
        }

        private IEnumerable<byte> SdParamsBytes(LogEventInfo logEvent)
        {
            return SdParams.SelectMany(sdParam => SpaceBytes.Concat(sdParam.Bytes(logEvent)));
        }
    }
}