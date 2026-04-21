using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Rpa.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IHttpClientBuilder AddScraperResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(2);
            options.Retry.UseJitter = true;

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }
}
