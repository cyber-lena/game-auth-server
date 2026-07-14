namespace GameAuth.Shared.Exceptions;

public class AuthException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public AuthException(string errorCode, string message, int statusCode = 400) 
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public AuthException(string errorCode, string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class ValidationException : AuthException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(Dictionary<string, string[]> validationErrors) 
        : base(Constants.ErrorCodes.ValidationFailed, "Validation failed", 400)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string field, string error)
        : base(Constants.ErrorCodes.ValidationFailed, "Validation failed", 400)
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

public class UnauthorizedException : AuthException
{
    public UnauthorizedException(string message = "Unauthorized") 
        : base(Constants.ErrorCodes.Unauthorized, message, 401)
    {
    }
}

public class ForbiddenException : AuthException
{
    public ForbiddenException(string message = "Forbidden") 
        : base(Constants.ErrorCodes.Forbidden, message, 403)
    {
    }
}

public class RateLimitException : AuthException
{
    public DateTime RetryAfter { get; }

    public RateLimitException(DateTime retryAfter, string message = "Rate limit exceeded") 
        : base(Constants.ErrorCodes.RateLimitExceeded, message, 429)
    {
        RetryAfter = retryAfter;
    }
}
