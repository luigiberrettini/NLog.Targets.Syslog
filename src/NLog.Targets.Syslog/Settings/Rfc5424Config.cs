// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc cref="NotifyPropertyChanged" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>RFC 5424 configuration</summary>
    [NLogConfigurationItem]
    public class Rfc5424Config : NotifyPropertyChanged, IDisposable
    {
        private const string DefaultVersion = "1";
        private const int DefaultTimestampFractionalDigits = 6;
        private const string NilValue = "-";
        private int timestampFractionalDigits;
        private Layout hostname;
        private Layout appName;
        private Layout procId;
        private Layout msgId;
        private StructuredDataConfig structuredData;
        private readonly PropertyChangedEventHandler structuredDataPropsChanged;
        private bool disableBom;

        /// <summary>The VERSION field of the HEADER part</summary>
        public string Version { get; }

        /// <summary>The number of fractional digits for the TIMESTAMP field of the HEADER part</summary>
        public int TimestampFractionalDigits
        {
            get => timestampFractionalDigits;
            set => SetProperty(ref timestampFractionalDigits, value);
        }

        /// <summary>The default HOSTNAME if no value is provided</summary>
        public string DefaultHostname { get; }

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname
        {
            get => hostname;
            set => SetProperty(ref hostname, value);
        }

        /// <summary>The default APPNAME if no value is provided</summary>
        public string DefaultAppName { get; }

        /// <summary>The APPNAME field of the HEADER part</summary>
        public Layout AppName
        {
            get => appName;
            set => SetProperty(ref appName, value);
        }

        /// <summary>The PROCID field of the HEADER part</summary>
        public Layout ProcId
        {
            get => procId;
            set => SetProperty(ref procId, value);
        }

        /// <summary>The MSGID field of the HEADER part</summary>
        public Layout MsgId
        {
            get => msgId;
            set => SetProperty(ref msgId, value);
        }

        /// <summary>The STRUCTURED-DATA part</summary>
        public StructuredDataConfig StructuredData
        {
            get => structuredData;
            set => SetProperty(ref structuredData, value);
        }

        /// <summary>Whether to remove or not BOM in the MSG part</summary>
        /// <see href="https://github.com/rsyslog/rsyslog/issues/284">RSyslog issue #284</see>
        public bool DisableBom
        {
            get => disableBom;
            set => SetProperty(ref disableBom, value);
        }

        /// <summary>Builds a new instance of the Rfc5424 class</summary>
        public Rfc5424Config()
        {
            Version = DefaultVersion;
            TimestampFractionalDigits = DefaultTimestampFractionalDigits;
            DefaultHostname = HostFqdn();
            hostname = DefaultHostname;
            DefaultAppName = UniversalAssembly.EntryAssembly().Name();
            appName = DefaultAppName;
            procId = NilValue;
            msgId = NilValue;
            structuredData = new StructuredDataConfig();
            structuredDataPropsChanged = (sender, args) => OnPropertyChanged(nameof(StructuredData));
            structuredData.PropertyChanged += structuredDataPropsChanged;
            disableBom = false;
        }

        /// <inheritdoc />
        /// <summary>Disposes the instance</summary>
        public void Dispose()
        {
            structuredData.PropertyChanged -= structuredDataPropsChanged;
            structuredData.Dispose();
        }

        private static string HostFqdn()
        {
            var hostname = Dns.GetHostName();
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var domainAsSuffix = $".{domainName}";
            return hostname.EndsWith(domainAsSuffix, StringComparison.InvariantCulture) ? hostname : $"{hostname}{domainAsSuffix}";
        }
    }
}