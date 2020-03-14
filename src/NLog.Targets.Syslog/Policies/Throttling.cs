// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Threading;
using NLog.Common;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class Throttling
    {
        public int Limit { get; }

        private ThrottlingStrategy Strategy { get; }

        private decimal Delay { get; }

        public bool BoundedBlockingCollectionNeeded =>
            Strategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
            Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
            Strategy == ThrottlingStrategy.Block;

        public static Throttling FromConfig(ThrottlingConfig throttlingConfig)
        {
            return new Throttling(throttlingConfig);
        }

        private Throttling(ThrottlingConfig throttlingConfig)
        {
            Limit = throttlingConfig.Limit;
            Strategy = throttlingConfig.Strategy;
            Delay = throttlingConfig.Delay;
        }

        public void Apply<T>(T entry, int waitingLogEntries, Action<T, int> processActionWithTimeout, Action<T> discardAction)
        {
            if (Strategy == ThrottlingStrategy.None || waitingLogEntries < Limit)
            {
                processActionWithTimeout(entry, Timeout.Infinite);
                return;
            }

            if (Strategy == ThrottlingStrategy.Discard)
            {
                InternalLogger.Warn("[Syslog] Applied discard throttling strategy");
                discardAction(entry);
                return;
            }

            ApplyDeferment(waitingLogEntries);
            processActionWithTimeout(entry, CalculateTimeout(waitingLogEntries));
        }

        private void ApplyDeferment(int waitingLogEntries)
        {
            var deferStrategy = Strategy == ThrottlingStrategy.DeferForFixedTime ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!deferStrategy)
                return;

            var deferment = FixedTime(Delay, waitingLogEntries);
            InternalLogger.Warn("[Syslog] Applying defer throttling strategy ({0} ms)", deferment);
            Thread.SpinWait(deferment);
        }

        private int CalculateTimeout(int waitingLogEntries)
        {
            var timeoutStrategy = Strategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
                Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout;

            if (!timeoutStrategy)
                return Timeout.Infinite;

            var timeout = FixedTime(Delay, waitingLogEntries);
            InternalLogger.Warn("[Syslog] Applying timeout throttling strategy ({0} ms)", timeout);
            return timeout;
        }

        private int FixedTime(decimal delay, int waitingLogEntries)
        {
            var isPercentageDelay = Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!isPercentageDelay)
                return (int)delay;

            var fixedTime = waitingLogEntries * delay / 100;
            return (int)fixedTime;
        }
    }
}