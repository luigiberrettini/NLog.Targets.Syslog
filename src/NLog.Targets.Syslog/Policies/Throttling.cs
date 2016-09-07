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

        internal void Apply(int waitingLogEntries, Action<int> action)
        {
            const int zeroMsDelay = 0;

            if (waitingLogEntries <= Limit)
            {
                action(zeroMsDelay);
                return;
            }

            if (Strategy == ThrottlingStrategy.Discard)
                return;

            ApplyDeferment(waitingLogEntries);
            action(ActionDelay(waitingLogEntries));

        }

        private void ApplyDeferment(int waitingLogEntries)
        {
            var deferStrategy = Strategy == ThrottlingStrategy.DeferForFixedTime ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!deferStrategy)
                return;

            var fixedDelay = FixedDelay(Delay, waitingLogEntries);
            Thread.SpinWait(fixedDelay);
        }

        private int ActionDelay(int waitingLogEntries)
        {
            const int zeroMsDelay = 0;
            const int infiniteDelay = -1;

            switch (Strategy)
            {
                case ThrottlingStrategy.None:
                case ThrottlingStrategy.DeferForFixedTime:
                case ThrottlingStrategy.DeferForPercentageTime:
                {
                    return FixedDelay(zeroMsDelay, waitingLogEntries);
                }
                case ThrottlingStrategy.DiscardOnFixedTimeout:
                case ThrottlingStrategy.DiscardOnPercentageTimeout:
                {
                    return FixedDelay(Delay, waitingLogEntries);
                }
                case ThrottlingStrategy.Block:
                {
                    return FixedDelay(infiniteDelay, waitingLogEntries);
                }
                default:
                {
                    throw new InvalidOperationException("LimitExceededAction value already evaluated");
                }
            }
        }

        private int FixedDelay(int delay, int waitingLogEntries)
        {
            var isPercentageDelay = Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;
            if (isPercentageDelay)
                return waitingLogEntries * delay / 100;
            return delay;
        }
    }
}