namespace GameAuth.Shared.DTOs.Responses;

public record AuthResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? SessionId { get; init; }
    public bool MfaRequired { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record TokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenType { get; init; }
}

public record ErrorResponse
{
    public required string ErrorCode { get; init; }
    public required string Message { get; init; }
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? TraceId { get; init; }
}

public record ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> ValidationErrors { get; init; } = new();
}
