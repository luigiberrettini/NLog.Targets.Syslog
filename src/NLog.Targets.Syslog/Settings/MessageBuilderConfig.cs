// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc cref="NotifyPropertyChanged" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>Message build configuration</summary>
    public class MessageBuilderConfig : NotifyPropertyChanged, IDisposable
    {
        private Facility facility;
        private LogLevelSeverityConfig perLogLevelSeverity;
        private readonly PropertyChangedEventHandler perLogLevelSeverityPropsChanged;
        private RfcNumber rfc;
        private Rfc3164Config rfc3164;
        private readonly PropertyChangedEventHandler rfc3164PropsChanged;
        private Rfc5424Config rfc5424;
        private readonly PropertyChangedEventHandler rfc5424PropsChanged;

        /// <summary>The Syslog facility to log from (its name e.g. local0 or local7)</summary>
        public Facility Facility
        {
            get => facility;
            set => SetProperty(ref facility, value);
        }

        /// <summary>Log level Syslog severity related fields</summary>
        public LogLevelSeverityConfig PerLogLevelSeverity
        {
            get => perLogLevelSeverity;
            set => SetProperty(ref perLogLevelSeverity, value);
        }

        /// <summary>The Syslog protocol RFC to be followed</summary>
        public RfcNumber Rfc
        {
            get => rfc;
            set => SetProperty(ref rfc, value);
        }

        /// <summary>RFC 3164 related fields</summary>
        public Rfc3164Config Rfc3164
        {
            get => rfc3164;
            set => SetProperty(ref rfc3164, value);
        }

        /// <summary>RFC 5424 related fields</summary>
        public Rfc5424Config Rfc5424
        {
            get => rfc5424;
            set => SetProperty(ref rfc5424, value);
        }

        /// <summary>Builds a new instance of the MessageBuilderConfig class</summary>
        public MessageBuilderConfig()
        {
            perLogLevelSeverity = new LogLevelSeverityConfig();
            perLogLevelSeverityPropsChanged = (sender, args) => OnPropertyChanged(nameof(PerLogLevelSeverity));

            rfc = RfcNumber.Rfc5424;

            rfc3164 = new Rfc3164Config();
            rfc3164PropsChanged = (sender, args) => OnPropertyChanged(nameof(Rfc3164));
            rfc3164.PropertyChanged += rfc3164PropsChanged;

            rfc5424 = new Rfc5424Config();
            rfc5424PropsChanged = (sender, args) => OnPropertyChanged(nameof(Rfc5424));
            rfc5424.PropertyChanged += rfc5424PropsChanged;
        }

        /// <inheritdoc />
        /// <summary>Disposes the instance</summary>
        public void Dispose()
        {
            perLogLevelSeverity.PropertyChanged -= perLogLevelSeverityPropsChanged;
            rfc3164.PropertyChanged -= rfc3164PropsChanged;
            rfc5424.PropertyChanged -= rfc5424PropsChanged;
            rfc5424.Dispose();
        }
    }
}