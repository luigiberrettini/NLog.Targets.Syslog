using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog SD-ID field of an SD-ELEMENT field</summary>
    public class SdId : SimpleLayout
    {
        private static readonly InternalLogDuplicatesPolicy DuplicatesPolicy;
        private SdIdPolicySet sdIdPolicySet;

        static SdId()
        {
            DuplicatesPolicy = new InternalLogDuplicatesPolicy();
        }

        /// <summary>Builds a new instance of the SdId class</summary>
        public SdId() : this(string.Empty)
        {
        }

        /// <summary>Builds a new instance of the SdId class</summary>
        /// <param name="text">The layout string to parse</param>
        public SdId(string text) : base(text)
        {
        }

        internal void Initialize(Enforcement enforcement)
        {
            sdIdPolicySet = new SdIdPolicySet(enforcement);
        }

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

        /// <summary>Gives a string representation of an SdId instance</summary>
        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return Render(nullEvent);
        }
    }
}