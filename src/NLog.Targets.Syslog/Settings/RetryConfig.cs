// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;

namespace NLog.Targets.Syslog.Settings
{
    /// <inheritdoc cref="NotifyPropertyChanged" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>Retry configuration</summary>
    public class RetryConfig : NotifyPropertyChanged, IDisposable
    {
        private const int DefaultMax = -1; // infinite retries
        private int max;
        private BackoffType backoff;
        private ConstantBackoffConfig constantBackoff;
        private readonly PropertyChangedEventHandler constantBackoffPropsChanged;
        private LinearBackoffConfig linearBackoff;
        private readonly PropertyChangedEventHandler linearBackoffPropsChanged;
        private ExponentialBackoffConfig exponentialBackoff;
        private readonly PropertyChangedEventHandler exponentialBackoffPropsChanged;
        private AwsJitteredExponentialBackoffConfig awsJitteredExponentialBackoff;
        private readonly PropertyChangedEventHandler awsJitteredExponentialBackoffPropsChanged;
        private PollyJitteredExponentialBackoffConfig pollyJitteredExponentialBackoff;
        private readonly PropertyChangedEventHandler pollyJitteredExponentialBackoffPropsChanged;

        internal bool InfiniteRetries => Max == DefaultMax;

        /// <summary>The maximum number of retries to perform after the first attempt failed</summary>
        public int Max
        {
            get => max;
            set => SetProperty(ref max, value <= 0 ? DefaultMax : value);
        }

        /// <summary>The backoff approach used to wait before performing a retry</summary>
        public BackoffType Backoff
        {
            get => backoff;
            set => SetProperty(ref backoff, value);
        }

        /// <summary>Constant back-off related fields</summary>
        public ConstantBackoffConfig ConstantBackoff
        {
            get => constantBackoff;
            set => SetProperty(ref constantBackoff, value);
        }

        /// <summary>Linear back-off related fields</summary>
        public LinearBackoffConfig LinearBackoff
        {
            get => linearBackoff;
            set => SetProperty(ref linearBackoff, value);
        }

        /// <summary>Exponential back-off related fields</summary>
        public ExponentialBackoffConfig ExponentialBackoff
        {
            get => exponentialBackoff;
            set => SetProperty(ref exponentialBackoff, value);
        }

        /// <summary>Aws jittered exponential back-off related fields</summary>
        public AwsJitteredExponentialBackoffConfig AwsJitteredExponentialBackoff
        {
            get => awsJitteredExponentialBackoff;
            set => SetProperty(ref awsJitteredExponentialBackoff, value);
        }

        /// <summary>Polly jittered exponential back-off related fields</summary>
        public PollyJitteredExponentialBackoffConfig PollyJitteredExponentialBackoff
        {
            get => pollyJitteredExponentialBackoff;
            set => SetProperty(ref pollyJitteredExponentialBackoff, value);
        }

        /// <summary>Builds a new instance of the RetryConfig class</summary>
        public RetryConfig()
        {
            max = DefaultMax;
            Backoff = BackoffType.Constant;

            constantBackoff = new ConstantBackoffConfig();
            constantBackoffPropsChanged = (sender, args) => OnPropertyChanged(nameof(ConstantBackoff));
            constantBackoff.PropertyChanged += constantBackoffPropsChanged;

            linearBackoff = new LinearBackoffConfig();
            linearBackoffPropsChanged = (sender, args) => OnPropertyChanged(nameof(LinearBackoff));
            linearBackoff.PropertyChanged += linearBackoffPropsChanged;

            exponentialBackoff = new ExponentialBackoffConfig();
            exponentialBackoffPropsChanged = (sender, args) => OnPropertyChanged(nameof(ExponentialBackoff));
            exponentialBackoff.PropertyChanged += exponentialBackoffPropsChanged;

            awsJitteredExponentialBackoff = new AwsJitteredExponentialBackoffConfig();
            awsJitteredExponentialBackoffPropsChanged = (sender, args) => OnPropertyChanged(nameof(AwsJitteredExponentialBackoff));
            awsJitteredExponentialBackoff.PropertyChanged += awsJitteredExponentialBackoffPropsChanged;

            pollyJitteredExponentialBackoff = new PollyJitteredExponentialBackoffConfig();
            pollyJitteredExponentialBackoffPropsChanged = (sender, args) => OnPropertyChanged(nameof(PollyJitteredExponentialBackoff));
            pollyJitteredExponentialBackoff.PropertyChanged += pollyJitteredExponentialBackoffPropsChanged;
        }

        /// <inheritdoc />
        /// <summary>Disposes the instance</summary>
        public void Dispose()
        {
            constantBackoff.PropertyChanged -= constantBackoffPropsChanged;
            linearBackoff.PropertyChanged -= linearBackoffPropsChanged;
            exponentialBackoff.PropertyChanged -= exponentialBackoffPropsChanged;
            awsJitteredExponentialBackoff.PropertyChanged -= awsJitteredExponentialBackoffPropsChanged;
            pollyJitteredExponentialBackoff.PropertyChanged -= pollyJitteredExponentialBackoffPropsChanged;
        }
    }
}