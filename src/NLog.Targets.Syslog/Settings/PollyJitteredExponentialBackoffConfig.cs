// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>Polly jittered exponential back-off configuration</summary>
    /// <remarks>At retry N the delay is min((next - prev) * 1/1.4 * <see cref="BaseDelay">BaseDelay</see>, maxTimeSpan) (first retry can be immediate, prev = 0 and then next, next = 2^t * tanh(sqrt(4*t)) and t = N + randomDouble)</remarks>
    public class PollyJitteredExponentialBackoffConfig : NotifyPropertyChanged
    {
        private const int DefaultBaseDelay = 500;
        private const int DefaultMaxDelay = 60000;
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
            set => SetProperty(ref baseDelay, value < 0 || value > maxDelay ? DefaultBaseDelay : value);
        }

        /// <summary>The maximum number of milliseconds used to compute the interval after which a retry is performed</summary>
        public int MaxDelay
        {
            get => maxDelay;
            set => SetProperty(ref maxDelay, value < 0 || value < baseDelay ? DefaultMaxDelay : value);
        }

        /// <summary>Builds a new instance of the PollyJitteredExponentialBackoffConfig class</summary>
        public PollyJitteredExponentialBackoffConfig()
        {
            firstDelayZero = false;
            baseDelay = DefaultBaseDelay;
            maxDelay = DefaultMaxDelay;
        }
    }
}