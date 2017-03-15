// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Throttling configuration</summary>
    public class ThrottlingConfig : NotifyPropertyChanged
    {
        private int limit;
        private ThrottlingStrategy strategy;
        private decimal delay;

        /// <summary>The number of log entries, waiting to be processed, that triggers throttling</summary>
        public int Limit
        {
            get
            {
                return limit;
            }
            set
            {

                if (value >= 1)
                {
                    SetProperty(ref limit, value);
                    return;
                }
                SetProperty(ref limit, 0);
                Strategy = ThrottlingStrategy.None;
            }
        }

        /// <summary>The throttling strategy to employ</summary>
        public ThrottlingStrategy Strategy
        {
            get { return strategy; }
            set { SetProperty(ref strategy, value); }
        }

        /// <summary>The milliseconds/percentage delay for a DiscardOnFixedTimeout/DiscardOnPercentageTimeout/Defer throttling strategy</summary>
        public decimal Delay
        {
            get { return delay; }
            set { SetProperty(ref delay, value < 0 ? 0 : value); }
        }

        /// <summary>Builds a new instance of the Throttling class</summary>
        public ThrottlingConfig()
        {
            limit = 0;
            strategy = ThrottlingStrategy.None;
            delay = 0;
        }
    }
}