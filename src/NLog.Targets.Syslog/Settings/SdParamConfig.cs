// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Syslog SD-PARAM field configuration</summary>
    public class SdParamConfig : NotifyPropertyChanged
    {
        private Layout name;
        private Layout value;

        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }
    }
}