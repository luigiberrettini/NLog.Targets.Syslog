using System;
using System.Threading;

namespace NLog.Targets.Syslog.Policies
{
    public class Throttling
    {
        /// <summary>The number of log entries, waiting to be processed, that triggers throttling</summary>
        public int Limit { get; set; }

        /// <summary>The throttling strategy to employ</summary>
        public ThrottlingStrategy Strategy { get; set; }

        /// <summary>The milliseconds/percentage delay for a DiscardOnFixedTimeout/DiscardOnPercentageTimeout/Defer throttling strategy</summary>
        public int Delay { get; set; }

        /// <summary>Builds a new instance of the Throttling class</summary>
        public Throttling()
        {
            Limit = 0;
            Strategy = ThrottlingStrategy.None;
            Delay = 0;
        }

        internal void EnsureAllowedSettings()
        {
            if (Limit < 1)
                Limit = 0;
            if (Delay < 1)
                Delay = 0;
            if (Limit == 0)
                Strategy = ThrottlingStrategy.None;
        }

        internal void Apply(int waitingLogEntries, Action<int> actionWithTimeout)
        {
            if (waitingLogEntries <= Limit)
            {
                actionWithTimeout(0);
                return;
            }

            if (Strategy == ThrottlingStrategy.Discard)
                return;

            ApplyDeferment(waitingLogEntries);

            var timeout = CalculateTimeout(waitingLogEntries, actionWithTimeout);
            actionWithTimeout(timeout);
        }

        private void ApplyDeferment(int waitingLogEntries)
        {
            var deferStrategy = Strategy == ThrottlingStrategy.DeferForFixedTime ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!deferStrategy)
                return;

            var delay = FixedTime(Delay, waitingLogEntries);
            Thread.SpinWait(delay);
        }

        private int CalculateTimeout(int waitingLogEntries, Action<int> actionWithTimeout)
        {
            var timeoutStrategy = Strategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
                Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout;

            return FixedTime(timeoutStrategy ? Delay : Timeout.Infinite, waitingLogEntries);
        }

        private int FixedTime(int delay, int waitingLogEntries)
        {
            var isPercentageDelay = Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;
            if (isPercentageDelay)
                return waitingLogEntries * delay / 100;
            return delay;
        }
    }
}