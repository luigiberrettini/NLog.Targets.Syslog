// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using System.Net;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>RFC 3164 configuration</summary>
    public class Rfc3164Config
    {
        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname { get; set; }

        /// <summary>The TAG field of the MSG part</summary>
        public Layout Tag { get; set; }

        /// <summary>Builds a new instance of the Rfc3164 class</summary>
        public Rfc3164Config()
        {
            Hostname = Dns.GetHostName();
            Tag = UniversalAssembly.EntryAssembly().Name();
        }
    }
}