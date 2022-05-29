// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>The throttling strategy to be used</summary>
    public enum BackoffType
    {
        /// <summary>Constant delay</summary>
        Constant,

        /// <summary>Linearly increasing delay</summary>
        Linear,

        /// <summary>Exponentially increasing delay</summary>
        Exponential,

        /// <summary>Exponentially increasing delay with decorrelated jitter (<see href="https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter">AWS formula</see>)</summary>
        AwsJitteredExponential,

        /// <summary>Exponentially increasing delay with decorrelated jitter (<see href="https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation">Polly formula</see>)</summary>
        PollyJitteredExponential
    }
}