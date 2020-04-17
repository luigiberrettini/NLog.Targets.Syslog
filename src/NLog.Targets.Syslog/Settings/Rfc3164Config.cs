// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using System.Net;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>RFC 3164 configuration</summary>
    public class Rfc3164Config : NotifyPropertyChanged
    {
        private bool outputPri;
        private bool outputHeader;
        private Layout hostname;
        private Layout timestamp = "${date:format=MMM d HH\\:mm\\:ss}";
        private Layout tag;

        /// <summary>Whether to output or not the PRI part</summary>
        public bool OutputPri
        {
            get => outputPri;
            set => SetProperty(ref outputPri, value);
        }

        /// <summary>Whether to output or not the HEADER part</summary>
        public bool OutputHeader
        {
            get => outputHeader;
            set => SetProperty(ref outputHeader, value);
        }

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname
        {
            get => hostname;
            set => SetProperty(ref hostname, value);
        }

        /// <summary>The TIMESTAMP field of the HEADER part</summary>
        public Layout Timestamp
        {
            get => timestamp;
            set => SetProperty(ref timestamp, value);
        }

        /// <summary>The TAG field of the MSG part</summary>
        public Layout Tag
        {
            get => tag;
            set => SetProperty(ref tag, value);
        }

        /// <summary>Builds a new instance of the Rfc3164 class</summary>
        public Rfc3164Config()
        {
            outputPri = true;
            outputHeader = true;
            hostname = Dns.GetHostName();
            tag = UniversalAssembly.EntryAssembly().Name();
        }
    }
}