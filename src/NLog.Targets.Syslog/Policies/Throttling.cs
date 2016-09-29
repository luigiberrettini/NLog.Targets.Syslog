using System;
using System.Threading;
using NLog.Common;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    public class Throttling
    {
        private readonly int limit;
        private readonly ThrottlingStrategy strategy;
        private readonly decimal delay;

        public static Throttling FromConfig(ThrottlingConfig throttlingConfig)
        {
            return new Throttling(throttlingConfig);
        }

        private Throttling(ThrottlingConfig throttlingConfig)
        {
            limit = throttlingConfig.Limit;
            strategy = throttlingConfig.Strategy;
            delay = throttlingConfig.Delay;
        }

        public void Apply(int waitingLogEntries, Action<int> actionWithTimeout)
        {
            if (strategy == ThrottlingStrategy.None || waitingLogEntries < limit)
            {
                actionWithTimeout(0);
                return;
            }

            if (strategy == ThrottlingStrategy.Discard)
            {
                InternalLogger.Warn("Applied discard throttling strategy");
                return;
            }

            ApplyDeferment(waitingLogEntries);
            actionWithTimeout(CalculateTimeout(waitingLogEntries));
        }

        private void ApplyDeferment(int waitingLogEntries)
        {
            var deferStrategy = strategy == ThrottlingStrategy.DeferForFixedTime ||
                strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!deferStrategy)
                return;

            var deferment = FixedTime(delay, waitingLogEntries);
            InternalLogger.Warn($"Applying defer throttling strategy ({deferment} ms)");
            Thread.SpinWait(deferment);
        }

        private int CalculateTimeout(int waitingLogEntries)
        {
            var timeoutStrategy = strategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
                strategy == ThrottlingStrategy.DiscardOnPercentageTimeout;

            if (!timeoutStrategy)
                return Timeout.Infinite;

            var timeout = FixedTime(delay, waitingLogEntries);
            InternalLogger.Warn($"Applying timeout throttling strategy ({timeout} ms)");
            return timeout;
        }

        private int FixedTime(decimal dlay, int waitingLogEntries)
        {
            var isPercentageDelay = strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!isPercentageDelay)
                return (int)dlay;

            var fixedTime = waitingLogEntries * dlay / 100;
            return (int)fixedTime;
        }
    }
}