// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Config;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog SD-ELEMENT field configuration</summary>
    public class SdElementConfig
    {
        /// <summary>The SD-ID field of an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        public SdIdConfig SdId { get; set; }

        /// <summary>The SD-PARAM fields belonging to an SD-ELEMENT field in the STRUCTURED-DATA part</summary>
        [ArrayParameter(typeof(SdParamConfig), "SdParam")]
        public IList<SdParamConfig> SdParams { get; set; }

        /// <summary>Builds a new instance of the SdElement class</summary>
        public SdElementConfig()
        {
            SdId = new SdIdConfig();
            SdParams = new List<SdParamConfig>();
        }
    }
}