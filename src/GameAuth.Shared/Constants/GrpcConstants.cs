namespace GameAuth.Shared.Constants;

public static class GrpcConstants
{
    public static class ServiceNames
    {
        public const string AuthService = "GameAuth.Auth";
        public const string ProfileService = "GameAuth.Profile";
        public const string AuditService = "GameAuth.Audit";
    }

    public static class Methods
    {
        public const string Login = "Login";
        public const string Register = "Register";
        public const string ValidateToken = "ValidateToken";
        public const string RefreshToken = "RefreshToken";
        public const string Logout = "Logout";
        public const string GetProfile = "GetProfile";
        public const string UpdateProfile = "UpdateProfile";
        public const string LogEvent = "LogEvent";
        public const string QueryLogs = "QueryLogs";
    }

    public static class Metadata
    {
        public const string CorrelationId = "correlation-id";
        public const string UserId = "user-id";
        public const string TraceId = "trace-id";
        public const string SpanId = "span-id";
    }
}
