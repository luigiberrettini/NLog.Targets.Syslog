using NLog.Layouts;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public class SdId : SimpleLayout
    {
        private static readonly InternalLogDuplicatesPolicy DuplicatesPolicy;
        private SdIdPolicySet sdIdPolicySet;

        static SdId()
        {
            DuplicatesPolicy = new InternalLogDuplicatesPolicy();
        }

        /// <summary>Initializes the SdId</summary>
        /// <param name="enforcement">The enforcement to apply</param>
        internal void Initialize(Enforcement enforcement)
        {
            sdIdPolicySet = new SdIdPolicySet(enforcement);
        }

        internal static IEnumerable<string> Render(IEnumerable<SdId> sdIds, LogEventInfo logEvent)
        {
            return sdIds.Select(x => x.Render(logEvent));
        }

        /// <summary>Gives the binary representation of a list of SdId</summary>
        /// <param name="sdIds">The list of SD-ID</param>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="encodings">The encodings to be used</param>
        /// <returns>Bytes containing the STRUCTURED_DATA part</returns>
        internal static IEnumerable<IEnumerable<byte>> Bytes(IEnumerable<SdId> sdIds, LogEventInfo logEvent, EncodingSet encodings)
        {
            return InternalLogDuplicatesPolicy.Apply(sdIds, x => x.Render(logEvent))
                .Select(x => x.Bytes(logEvent, encodings));
        }

        private IEnumerable<byte> Bytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            var sdId = sdIdPolicySet.Apply(Render(logEvent));
            return encodings.Ascii.GetBytes(sdId);
        }
    }
}