// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>Exponential back-off configuration</summary>
    /// <remarks>At retry N the delay is <see cref="BaseDelay">BaseDelay</see> * <see cref="ScaleFactor">ScaleFactor</see>^N (first retry can be immediate)</remarks>
    public class ExponentialBackoffConfig : NotifyPropertyChanged
    {
        private const int DefaultBaseDelay = 500;
        private const double DefaultScaleFactor = 2.0;
        private bool firstDelayZero;
        private int baseDelay;
        private double scaleFactor;

        /// <summary>Whether the first retry should be performed immediately</summary>
        public bool FirstDelayZero
        {
            get => firstDelayZero;
            set => SetProperty(ref firstDelayZero, value);
        }

        /// <summary>The number of milliseconds used as the base to compute the interval after which a retry is performed</summary>
        /// <remarks>Must be greater than or equal to 0</remarks>
        public int BaseDelay
        {
            get => baseDelay;
            set => SetProperty(ref baseDelay, value < 0 ? DefaultBaseDelay : value);
        }

        /// <summary>The scale factor used to compute the interval after which a retry is performed</summary>
        /// <remarks>Must be greater than or equal to 1</remarks>
        public double ScaleFactor
        {
            get => scaleFactor;
            set => SetProperty(ref scaleFactor, value < 1d ? DefaultScaleFactor : value);
        }

        /// <summary>Builds a new instance of the ExponentialBackoffConfig class</summary>
        public ExponentialBackoffConfig()
        {
            firstDelayZero = false;
            baseDelay = DefaultBaseDelay;
            scaleFactor = DefaultScaleFactor;
        }
    }
}