using System.Net.Sockets;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace GameAuth.ServiceDefaults.Resilience;

/// <summary>
/// Registers shared Polly resilience pipelines (retry with exponential backoff + jitter,
/// circuit breaker, and timeouts) and a standard resilience handler for typed HttpClients.
/// The named pipeline can be resolved for gRPC calls or any custom outbound operation.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>Key of the shared resilience pipeline registered in the <see cref="ResiliencePipelineProvider{TKey}"/>.</summary>
    public const string DefaultPipelineKey = "gameauth-default";

    public static IServiceCollection AddResilienceDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>()
                      ?? new ResilienceOptions();

        services.AddSingleton(options);

        services.AddResiliencePipeline(DefaultPipelineKey, builder =>
        {
            ConfigurePipeline(builder, options);
        });

        return services;
    }

    /// <summary>
    /// Registers a typed gRPC client with the shared resilience options applied to its
    /// underlying HttpClient (retry + circuit breaker + timeout). Use for inter-service calls.
    /// </summary>
    public static IHttpClientBuilder AddResilientGrpcClient<TClient>(
        this IServiceCollection services,
        IConfiguration configuration,
        Uri address)
        where TClient : class
    {
        var options = configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>()
                      ?? new ResilienceOptions();

        return services
            .AddGrpcClient<TClient>(o => o.Address = address)
            .AddResilienceHandlerDefaults(options);
    }

    /// <summary>
    /// Adds the standard HTTP resilience handler (retry + circuit breaker + timeout) to a
    /// typed HttpClient registration, aligned with the shared <see cref="ResilienceOptions"/>.
    /// </summary>
    public static IHttpClientBuilder AddResilienceHandlerDefaults(
        this IHttpClientBuilder builder,
        ResilienceOptions options)
    {
        builder.AddStandardResilienceHandler(o =>
        {
            o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.AttemptTimeoutSeconds);
            o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TotalTimeoutSeconds);

            o.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
            o.Retry.Delay = TimeSpan.FromSeconds(options.BaseRetryDelaySeconds);
            o.Retry.BackoffType = DelayBackoffType.Exponential;
            o.Retry.UseJitter = true;

            o.CircuitBreaker.FailureRatio = options.CircuitBreakerFailureRatio;
            o.CircuitBreaker.MinimumThroughput = options.CircuitBreakerMinimumThroughput;
            o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerSamplingDurationSeconds);
            o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds);
        });

        return builder;
    }

    private static void ConfigurePipeline(ResiliencePipelineBuilder builder, ResilienceOptions options)
    {
        // Overall timeout guarding the entire operation (retries included).
        builder.AddTimeout(TimeSpan.FromSeconds(options.TotalTimeoutSeconds));

        builder.AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(IsTransient),
            MaxRetryAttempts = options.MaxRetryAttempts,
            Delay = TimeSpan.FromSeconds(options.BaseRetryDelaySeconds),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        });

        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(IsTransient),
            FailureRatio = options.CircuitBreakerFailureRatio,
            MinimumThroughput = options.CircuitBreakerMinimumThroughput,
            SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerSamplingDurationSeconds),
            BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds)
        });

        // Timeout for each individual attempt.
        builder.AddTimeout(TimeSpan.FromSeconds(options.AttemptTimeoutSeconds));
    }

    /// <summary>
    /// Determines whether an exception represents a transient fault worth retrying:
    /// network/socket failures, timeouts, and retryable gRPC status codes. Non-transient
    /// application errors (e.g. gRPC InvalidArgument, NotFound, Unauthenticated) are not retried.
    /// </summary>
    private static bool IsTransient(Exception ex) => ex switch
    {
        RpcException rpc => IsTransient(rpc.StatusCode),
        TimeoutRejectedException => true,
        BrokenCircuitException => false,
        SocketException => true,
        HttpRequestException => true,
        _ => false
    };

    private static bool IsTransient(StatusCode status) => status switch
    {
        StatusCode.Unavailable => true,
        StatusCode.DeadlineExceeded => true,
        StatusCode.ResourceExhausted => true,
        StatusCode.Aborted => true,
        StatusCode.Internal => true,
        _ => false
    };
}
