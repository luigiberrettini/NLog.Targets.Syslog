using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog SD-PARAM field configuration</summary>
    public class SdParamConfig
    {
        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name { get; set; }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value { get; set; }
    }
}