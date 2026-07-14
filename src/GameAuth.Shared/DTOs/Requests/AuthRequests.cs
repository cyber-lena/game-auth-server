namespace GameAuth.Shared.DTOs.Requests;

public record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RequireMfa { get; init; }
    public string? MfaCode { get; init; }
}

public record RegisterRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public record LogoutRequest
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
}

public record MfaChallengeRequest
{
    public required string SessionId { get; init; }
    public required string MfaCode { get; init; }
}
