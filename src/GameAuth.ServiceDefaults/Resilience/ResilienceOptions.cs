namespace GameAuth.ServiceDefaults.Resilience;

/// <summary>
/// Tunable parameters for the shared resilience pipeline, bound from the
/// "Resilience" configuration section. Sensible production defaults are used
/// when the section (or an individual value) is absent.
/// </summary>
public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    /// <summary>Maximum number of retry attempts (in addition to the initial call).</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Base delay (seconds) for the exponential backoff retry strategy.</summary>
    public double BaseRetryDelaySeconds { get; set; } = 0.5;

    /// <summary>Timeout (seconds) applied per individual attempt.</summary>
    public double AttemptTimeoutSeconds { get; set; } = 10;

    /// <summary>Total timeout (seconds) for the whole operation including retries.</summary>
    public double TotalTimeoutSeconds { get; set; } = 30;

    /// <summary>Failure ratio (0-1) within the sampling window that trips the circuit breaker.</summary>
    public double CircuitBreakerFailureRatio { get; set; } = 0.5;

    /// <summary>Minimum number of actions in the sampling window before the breaker can trip.</summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>Sampling window (seconds) over which failures are measured.</summary>
    public double CircuitBreakerSamplingDurationSeconds { get; set; } = 30;

    /// <summary>Duration (seconds) the circuit stays open before allowing a trial call.</summary>
    public double CircuitBreakerBreakDurationSeconds { get; set; } = 15;
}
