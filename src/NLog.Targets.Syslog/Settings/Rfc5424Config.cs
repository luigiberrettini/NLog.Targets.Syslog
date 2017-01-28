// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using System.Net;
using System.Net.NetworkInformation;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>RFC 5424 configuration</summary>
    public class Rfc5424Config
    {
        private const string DefaultVersion = "1";
        private const string NilValue = "-";

        /// <summary>The VERSION field of the HEADER part</summary>
        public string Version { get; }

        /// <summary>The default HOSTNAME if no value is provided</summary>
        public string DefaultHostname { get; }

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname { get; set; }

        /// <summary>The default APPNAME if no value is provided</summary>
        public string DefaultAppName { get; }

        /// <summary>The APPNAME field of the HEADER part</summary>
        public Layout AppName { get; set; }

        /// <summary>The PROCID field of the HEADER part</summary>
        public Layout ProcId { get; set; }

        /// <summary>The MSGID field of the HEADER part</summary>
        public Layout MsgId { get; set; }

        /// <summary>The STRUCTURED-DATA part</summary>
        public StructuredDataConfig StructuredData { get; set; }

        /// <summary>Whether to remove or not BOM in the MSG part</summary>
        /// <see href="https://github.com/rsyslog/rsyslog/issues/284">RSyslog issue #284</see>
        public bool DisableBom { get; set; }

        /// <summary>Builds a new instance of the Rfc5424 class</summary>
        public Rfc5424Config()
        {
            DefaultHostname = HostFqdn();
            DefaultAppName = UniversalAssembly.EntryAssembly().Name();
            Version = DefaultVersion;
            Hostname = DefaultHostname;
            AppName = DefaultAppName;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = new StructuredDataConfig();
            DisableBom = false;
        }

        private static string HostFqdn()
        {
            var hostname = Dns.GetHostName();
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var domainAsSuffix = $".{domainName}";
            return hostname.EndsWith(domainAsSuffix) ? hostname : $"{hostname}{domainAsSuffix}";
        }
    }
}