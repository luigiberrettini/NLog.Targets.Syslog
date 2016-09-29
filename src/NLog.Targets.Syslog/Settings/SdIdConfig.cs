using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog SD-ID field configuration</summary>
    public class SdIdConfig : SimpleLayout
    {
        /// <summary>Builds a new instance of the SdId class</summary>
        public SdIdConfig() : this(string.Empty)
        {
        }

        /// <summary>Builds a new instance of the SdId class</summary>
        /// <param name="text">The layout string to parse</param>
        public SdIdConfig(string text) : base(text)
        {
        }

        /// <summary>Converts a string to a new instance of the SdId class</summary>
        /// <param name="text">The layout string to parse</param>
        /// <remarks>Needed to parse NLog configuration</remarks>
        public static implicit operator SdIdConfig(string text)
        {
            return new SdIdConfig(text);
        }
    }
}