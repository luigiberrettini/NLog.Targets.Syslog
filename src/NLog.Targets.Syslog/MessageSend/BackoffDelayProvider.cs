// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Settings;
using Polly.Contrib.WaitAndRetry;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class BackoffDelayProvider
    {
        private static readonly Dictionary<BackoffType, Func<RetryConfig, BackoffDelayProvider>> BackoffFactory;
        private readonly IEnumerable<TimeSpan> delaysEnumerable;
        private IEnumerator<TimeSpan> delaysEnumerator;

        static BackoffDelayProvider()
        {
            BackoffFactory = new Dictionary<BackoffType, Func<RetryConfig, BackoffDelayProvider>>
            {
                { BackoffType.Constant, retryConfig => new BackoffDelayProvider(PollyMaxRetries(retryConfig), retryConfig.ConstantBackoff) },
                { BackoffType.Linear, retryConfig => new BackoffDelayProvider(PollyMaxRetries(retryConfig), retryConfig.LinearBackoff) },
                { BackoffType.Exponential, retryConfig => new BackoffDelayProvider(PollyMaxRetries(retryConfig), retryConfig.ExponentialBackoff) },
                { BackoffType.AwsJitteredExponential, retryConfig => new BackoffDelayProvider(PollyMaxRetries(retryConfig), retryConfig.AwsJitteredExponentialBackoff) },
                { BackoffType.PollyJitteredExponential, retryConfig => new BackoffDelayProvider(PollyMaxRetries(retryConfig), retryConfig.PollyJitteredExponentialBackoff) },
            };
        }

        public static BackoffDelayProvider FromConfig(RetryConfig retryConfig)
        {
            return BackoffFactory[retryConfig.Backoff](retryConfig);
        }

        private static int PollyMaxRetries(RetryConfig retryConfig)
        {
            return retryConfig.InfiniteRetries ? int.MaxValue : retryConfig.Max;
        }

        private BackoffDelayProvider(int maxRetries, ConstantBackoffConfig config)
        {
            var baseDelay = TimeSpan.FromMilliseconds(config.BaseDelay);
            delaysEnumerable = Backoff.ConstantBackoff(baseDelay, maxRetries, config.FirstDelayZero);
        }

        private BackoffDelayProvider(int maxRetries, LinearBackoffConfig config)
        {
            var baseDelay = TimeSpan.FromMilliseconds(config.BaseDelay);
            delaysEnumerable = Backoff.LinearBackoff(baseDelay, maxRetries, config.ScaleFactor, config.FirstDelayZero);
        }

        private BackoffDelayProvider(int maxRetries, ExponentialBackoffConfig config)
        {
            var baseDelay = TimeSpan.FromMilliseconds(config.BaseDelay);
            delaysEnumerable = Backoff.ExponentialBackoff(baseDelay, maxRetries, config.ScaleFactor, config.FirstDelayZero);
        }

        private BackoffDelayProvider(int maxRetries, AwsJitteredExponentialBackoffConfig config)
        {
            var baseDelay = TimeSpan.FromMilliseconds(config.BaseDelay);
            var maxDelay = TimeSpan.FromMilliseconds(config.MaxDelay);
            delaysEnumerable = Backoff.AwsDecorrelatedJitterBackoff(baseDelay, maxDelay, maxRetries, null, config.FirstDelayZero);
        }

        private BackoffDelayProvider(int maxRetries, PollyJitteredExponentialBackoffConfig config)
        {
            var baseDelay = TimeSpan.FromMilliseconds(config.BaseDelay);
            var maxDelay = TimeSpan.FromMilliseconds(config.MaxDelay);
            delaysEnumerable = Backoff
                .DecorrelatedJitterBackoffV2(baseDelay, maxRetries, null, config.FirstDelayZero)
                .Select(x => x < maxDelay ? x : maxDelay);
        }

        public TimeSpan GetDelay(bool isFirstRetry)
        {
            if (isFirstRetry)
                delaysEnumerator = delaysEnumerable.GetEnumerator();
            delaysEnumerator.MoveNext(); // result is not checked to allow infinite retries (Polly.Contrib.WaitAndRetry is limited to int.MaxValue retries)
            return delaysEnumerator.Current;
        }
    }
}