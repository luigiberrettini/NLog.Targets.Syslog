// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>Syslog SD-PARAM field configuration</summary>
    [NLogConfigurationItem]
    public class SdParamConfig : NotifyPropertyChanged
    {
        private Layout name;
        private Layout value;

        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
        }
    }
}