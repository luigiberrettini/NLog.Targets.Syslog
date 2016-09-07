using NLog.Layouts;
using NLog.Targets.Syslog.Policies;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog SD-ID field of an SD-ELEMENT field</summary>
    public class SdId : SimpleLayout
    {
        private SdIdPolicySet sdIdPolicySet;

        /// <summary>Builds a new instance of the SdId class</summary>
        public SdId() : this(string.Empty)
        {
        }

        /// <summary>Builds a new instance of the SdId class</summary>
        /// <param name="text">The layout string to parse</param>
        public SdId(string text) : base(text)
        {
        }

        /// <summary>Converts a string to a new instance of the SdId class</summary>
        /// <param name="text">The layout string to parse</param>
        /// <remarks>Needed during the NLog configuration parsing</remarks>
        public static implicit operator SdId(string text)
        {
            return new SdId(text);
        }

        internal void Initialize(Enforcement enforcement)
        {
            sdIdPolicySet = new SdIdPolicySet(enforcement);
        }

        internal void AppendBytes(ByteArray message, string renderedSdId, EncodingSet encodings)
        {
            var sdId = sdIdPolicySet.Apply(renderedSdId);
            var sdIdBytes = encodings.Ascii.GetBytes(sdId);
            message.Append(sdIdBytes);
        }

        /// <summary>Gives a string representation of an SdId instance</summary>
        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return Render(nullEvent);
        }
    }
}