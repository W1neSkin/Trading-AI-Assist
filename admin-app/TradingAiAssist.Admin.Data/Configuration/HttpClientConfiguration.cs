using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace TradingAiAssist.Admin.Data.Configuration
{
    public static class HttpClientConfiguration
    {
        public static IServiceCollection AddHttpClientsWithRetry(this IServiceCollection services, ApiOptions apiOptions)
        {
            // Configure retry policy
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: apiOptions.MaxRetries,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogWarning(
                            "Delaying for {Delay}ms before retry {RetryCount} for {RequestUri}",
                            timespan.TotalMilliseconds,
                            retryAttempt,
                            context.GetRequestUri());
                    });

            // Configure circuit breaker policy
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan, context) =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogWarning(
                            "Circuit breaker opened for {Duration}ms due to {Outcome}",
                            timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    },
                    onReset: context =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogInformation("Circuit breaker reset");
                    });

            // Configure timeout policy
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(apiOptions.TimeoutSeconds));

            // Combine policies
            var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

            // Configure named HTTP client
            services.AddHttpClient("TradingAiAssistApi", client =>
            {
                client.BaseAddress = new Uri(apiOptions.BaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "TradingAiAssist-Admin/1.0");
                client.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
            })
            .AddPolicyHandler(policyWrap)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                UseCookies = false
            });

            return services;
        }
    }

    public static class ContextExtensions
    {
        public static ILogger? GetLogger(this Context context)
        {
            return context.TryGetValue("logger", out var logger) ? logger as ILogger : null;
        }

        public static Uri? GetRequestUri(this Context context)
        {
            return context.TryGetValue("requestUri", out var uri) ? uri as Uri : null;
        }
    }
} 