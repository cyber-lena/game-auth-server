namespace GameAuth.Shared.Constants;

public static class ErrorCodes
{
    // Authentication errors (AUTH-1xxx)
    public const string InvalidCredentials = "AUTH-1001";
    public const string UserNotFound = "AUTH-1002";
    public const string UserAlreadyExists = "AUTH-1003";
    public const string InvalidToken = "AUTH-1004";
    public const string ExpiredToken = "AUTH-1005";
    public const string TokenRevoked = "AUTH-1006";
    public const string MfaRequired = "AUTH-1007";
    public const string InvalidMfaCode = "AUTH-1008";

    // Authorization errors (AUTH-2xxx)
    public const string Unauthorized = "AUTH-2001";
    public const string Forbidden = "AUTH-2002";
    public const string InsufficientPermissions = "AUTH-2003";

    // Rate limiting errors (RATE-3xxx)
    public const string RateLimitExceeded = "RATE-3001";
    public const string TooManyLoginAttempts = "RATE-3002";

    // Validation errors (VAL-4xxx)
    public const string ValidationFailed = "VAL-4001";
    public const string InvalidEmailFormat = "VAL-4002";
    public const string WeakPassword = "VAL-4003";
    public const string InvalidUsername = "VAL-4004";

    // Internal errors (SYS-5xxx)
    public const string InternalServerError = "SYS-5001";
    public const string DatabaseError = "SYS-5002";
    public const string CacheError = "SYS-5003";
    public const string MessageBusError = "SYS-5004";
}
