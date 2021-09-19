// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc />
    /// <summary>Constant back-off configuration</summary>
    /// <remarks>At retry N the delay is <see cref="BaseDelay">BaseDelay</see> (first retry can be immediate)</remarks>
    public class ConstantBackoffConfig : NotifyPropertyChanged
    {
        private const int DefaultBaseDelay = 500;
        private bool firstDelayZero;
        private int baseDelay;

        /// <summary>Whether the first retry should be performed immediately</summary>
        public bool FirstDelayZero
        {
            get => firstDelayZero;
            set => SetProperty(ref firstDelayZero, value);
        }

        /// <summary>The number of milliseconds used as the base to compute the interval after which a retry is performed</summary>
        /// <remarks>Must be greater than 0</remarks>
        public int BaseDelay
        {
            get => baseDelay;
            set => SetProperty(ref baseDelay, value <= 0 ? DefaultBaseDelay : value);
        }

        /// <summary>Builds a new instance of the ConstantBackoffConfig class</summary>
        public ConstantBackoffConfig()
        {
            firstDelayZero = false;
            baseDelay = DefaultBaseDelay;
        }
    }
}