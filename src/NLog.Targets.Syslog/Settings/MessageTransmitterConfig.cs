// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc cref="NotifyPropertyChanged" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>Message transmission configuration</summary>
    public class MessageTransmitterConfig : NotifyPropertyChanged, IDisposable
    {
        private ProtocolType protocol;
        private UdpConfig udp;
        private readonly PropertyChangedEventHandler udpPropsChanged;
        private TcpConfig tcp;
        private readonly PropertyChangedEventHandler tcpPropsChanged;

        /// <summary>The Syslog server protocol</summary>
        public ProtocolType Protocol
        {
            get => protocol;
            set => SetProperty(ref protocol, value);
        }

        /// <summary>UDP related fields</summary>
        public UdpConfig Udp
        {
            get => udp;
            set => SetProperty(ref udp, value);
        }

        /// <summary>TCP related fields</summary>
        public TcpConfig Tcp
        {
            get => tcp;
            set => SetProperty(ref tcp, value);
        }

        /// <summary>Builds a new instance of the MessageTransmitterConfig class</summary>
        public MessageTransmitterConfig()
        {
            udp = new UdpConfig();
            udpPropsChanged = (sender, args) => OnPropertyChanged(nameof(Udp));
            udp.PropertyChanged += udpPropsChanged;

            tcp = new TcpConfig();
            tcpPropsChanged = (sender, args) => OnPropertyChanged(nameof(Tcp));
            tcp.PropertyChanged += tcpPropsChanged;
        }

        /// <inheritdoc />
        /// <summary>Disposes the instance</summary>
        public void Dispose()
        {
            udp.PropertyChanged -= udpPropsChanged;
            tcp.PropertyChanged -= tcpPropsChanged;
            tcp.Dispose();
        }
    }
}