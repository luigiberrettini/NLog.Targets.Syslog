// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog STRUCTURED-DATA part configuration</summary>
    public class StructuredDataConfig
    {
        /// <summary>Allows to use log event properties data enabling different STRUCTURED-DATA for each log message</summary>
        public Layout FromEventProperties { get; set; }

        /// <summary>The SD-ELEMENTs contained in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdElementConfig), "SdElement")]
        public IList<SdElementConfig> SdElements { get; set; }

        /// <summary>Builds a new instance of the StructuredData class</summary>
        public StructuredDataConfig()
        {
            FromEventProperties = string.Empty;
            SdElements = new List<SdElementConfig>();
        }
    }
}