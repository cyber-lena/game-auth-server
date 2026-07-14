namespace GameAuth.Shared.Constants;

public static class AuthConstants
{
    public const string JwtBearerScheme = "Bearer";
    public const string AuthorizationHeader = "Authorization";
    public const string CorrelationIdHeader = "X-Correlation-ID";

    public static class Claims
    {
        public const string UserId = "user_id";
        public const string Username = "username";
        public const string Email = "email";
        public const string Role = "role";
        public const string SessionId = "session_id";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
    }

    public static class TokenTypes
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }
}
