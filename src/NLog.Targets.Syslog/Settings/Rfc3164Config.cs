// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using System.Net;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>RFC 3164 configuration</summary>
    public class Rfc3164Config : NotifyPropertyChanged
    {
        private Layout hostname;
        private Layout tag;

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname
        {
            get { return hostname; }
            set { SetProperty(ref hostname, value); }
        }

        /// <summary>The TAG field of the MSG part</summary>
        public Layout Tag
        {
            get { return tag; }
            set { SetProperty(ref tag, value); }
        }

        /// <summary>Builds a new instance of the Rfc3164 class</summary>
        public Rfc3164Config()
        {
            hostname = Dns.GetHostName();
            tag = UniversalAssembly.EntryAssembly().Name();
        }
    }
}