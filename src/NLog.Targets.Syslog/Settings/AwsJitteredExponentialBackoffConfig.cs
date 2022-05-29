// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>AWS jittered exponential back-off configuration</summary>
    /// <remarks>At retry N the delay is randomUniformBetween(<see cref="BaseDelay">BaseDelay</see>, min(<see cref="MaxDelay">MaxDelay</see>, previousDelay * 3)) (first retry can be immediate, previousDelay initialized to <see cref="BaseDelay">BaseDelay</see>)</remarks>
    public class AwsJitteredExponentialBackoffConfig : NotifyPropertyChanged
    {
        private const int DefaultBaseDelay = 500;
        private const int DefaultMaxDelay = 1500;
        private bool firstDelayZero;
        private int baseDelay;
        private int maxDelay;

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
            set => SetProperty(ref baseDelay, value < 0 || value > maxDelay ? maxDelay / 3 : value);
        }

        /// <summary>The maximum number of milliseconds used to compute the interval after which a retry is performed</summary>
        /// <remarks>Must be greater than or equal to <see cref="BaseDelay"/>BaseDelay</remarks>
        public int MaxDelay
        {
            get => maxDelay;
            set => SetProperty(ref maxDelay, value < 0 || value < baseDelay ? baseDelay * 3 : value);
        }

        /// <summary>Builds a new instance of the AwsJitteredExponentialBackoffConfig class</summary>
        public AwsJitteredExponentialBackoffConfig()
        {
            firstDelayZero = false;
            baseDelay = DefaultBaseDelay;
            maxDelay = DefaultMaxDelay;
        }
    }
}