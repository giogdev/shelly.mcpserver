using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using System.Net;

namespace Shelly.Services.Utils
{
    public static class HttpPolicies
    {
        /// <summary>
        /// Creates a retry policy that retries twice with a 1-second delay (Shelly Cloud rate limit).
        /// </summary>
        public static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> CreateStandardRetryPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(
                    retryCount: 2,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
                    onRetryAsync: async (outcome, timespan, retryCount, _) =>
                    {
                        if (outcome.Exception != null)
                        {
                            logger.LogWarning(outcome.Exception,
                                "HTTP request failed, retry {RetryCount} after {Delay}s",
                                retryCount, timespan.TotalSeconds);
                        }
                        else if (outcome.Result != null)
                        {
                            var body = await outcome.Result.Content.ReadAsStringAsync();
                            logger.LogWarning(
                                "HTTP request returned {StatusCode}, retry {RetryCount} after {Delay}s. Body: {Body}",
                                outcome.Result.StatusCode, retryCount, timespan.TotalSeconds, body);
                        }
                    });
        }

        /// <summary>
        /// Creates a circuit breaker that opens after 5 consecutive failures and stays open for 30 seconds.
        /// When open, all requests immediately fail with <see cref="BrokenCircuitException"/>.
        /// </summary>
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode != HttpStatusCode.OK)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                        logger.LogError(
                            outcome.Exception,
                            "Circuit breaker opened for {Duration}s after repeated failures. Last status: {StatusCode}",
                            duration.TotalSeconds,
                            outcome.Result?.StatusCode),
                    onReset: () =>
                        logger.LogInformation("Circuit breaker reset — upstream is healthy again."),
                    onHalfOpen: () =>
                        logger.LogInformation("Circuit breaker half-open — testing upstream with a probe request."));
        }

        /// <summary>
        /// Returns a wrapped policy: retry (inner) + circuit breaker (outer).
        /// The circuit breaker counts failures that survive all retries.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy(ILogger logger)
        {
            var retry          = CreateStandardRetryPolicy(logger);
            var circuitBreaker = CreateCircuitBreakerPolicy(logger);
            // Outer = circuit breaker; inner = retry. Order: request → CB → retry → HTTP.
            return Policy.WrapAsync(circuitBreaker, retry);
        }
    }
}
