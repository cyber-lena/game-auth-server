using GameAuth.Core.Configuration;
using GameAuth.Core.Security;
using GameAuth.Infrastructure.Caching;
using GameAuth.Infrastructure.Persistence.Entities;
using GameAuth.Infrastructure.Persistence.Repositories;
using GameAuth.Protos.Auth;
using GameAuth.Shared.Events;
using GameAuth.Shared.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace GameAuth.Core.Services;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly ISessionCacheService _sessionCache;
    private readonly ITokenRevocationService _tokenRevocation;
    private readonly IEventBus _eventBus;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService,
        ISessionCacheService sessionCache,
        ITokenRevocationService tokenRevocation,
        IEventBus eventBus,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthGrpcService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _sessionCache = sessionCache;
        _tokenRevocation = tokenRevocation;
        _eventBus = eventBus;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        if (await _users.UsernameExistsAsync(request.Username, context.CancellationToken))
        {
            return new RegisterResponse { Success = false, Message = "Username already taken" };
        }

        if (await _users.EmailExistsAsync(request.Email, context.CancellationToken))
        {
            return new RegisterResponse { Success = false, Message = "Email already registered" };
        }

        var user = new User { Username = request.Username, Email = request.Email };
        user.Credential = new Credential { PasswordHash = _passwordHasher.Hash(request.Password) };

        await _users.AddAsync(user, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        await _eventBus.PublishAsync(new UserRegisteredEvent
        {
            CorrelationId = context.GetHttpContext()?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        }, context.CancellationToken);

        return new RegisterResponse { Success = true, Message = "Registration successful", UserId = user.Id };
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _users.GetByUsernameAsync(request.Username, context.CancellationToken);

        if (user is null)
        {
            return new LoginResponse { Success = false, Message = "Invalid credentials" };
        }

        var fullUser = await _users.GetUserWithCredentialAsync(user.Id, context.CancellationToken);
        if (fullUser?.Credential is null || !_passwordHasher.Verify(request.Password, fullUser.Credential.PasswordHash))
        {
            return new LoginResponse { Success = false, Message = "Invalid credentials" };
        }

        var mfaUser = await _users.GetUserWithMfaSettingsAsync(user.Id, context.CancellationToken);
        if (mfaUser?.MfaSettings is { Verified: true })
        {
            if (string.IsNullOrEmpty(request.MfaCode))
            {
                return new LoginResponse { Success = false, Message = "MFA code required", MfaRequired = true };
            }

            if (!_mfaService.ValidateCode(mfaUser.MfaSettings.MfaSecret, request.MfaCode))
            {
                return new LoginResponse { Success = false, Message = "Invalid MFA code", MfaRequired = true };
            }
        }

        return await IssueSessionAsync(user, mfaUser?.MfaSettings is { Verified: true }, context);
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        if (await _tokenRevocation.IsTokenRevokedAsync(request.AccessToken))
        {
            return new ValidateTokenResponse { IsValid = false };
        }

        var principal = _tokenService.ValidateAccessToken(request.AccessToken);
        if (principal is null)
        {
            return new ValidateTokenResponse { IsValid = false };
        }

        var userId = long.TryParse(principal.FindFirst("sub")?.Value, out var id) ? id : 0;
        var response = new ValidateTokenResponse
        {
            IsValid = true,
            UserId = userId,
            Username = principal.FindFirst("unique_name")?.Value ?? string.Empty,
            SessionId = principal.FindFirst("sid")?.Value ?? string.Empty
        };
        return response;
    }

    public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            await _tokenRevocation.RevokeTokenAsync(request.AccessToken, TimeSpan.FromMinutes(_jwtOptions.AccessTokenMinutes));
        }

        if (!string.IsNullOrEmpty(request.SessionId))
        {
            await _sessionCache.RemoveSessionAsync(request.SessionId);
        }

        return new LogoutResponse { Success = true, Message = "Logged out" };
    }

    public override async Task<TokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        var session = await _sessionCache.GetSessionAsync(request.RefreshToken);
        if (session is null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid refresh token"));
        }

        var user = await _users.GetByIdAsync(session.UserId, context.CancellationToken);
        if (user is null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User not found"));
        }

        var access = _tokenService.GenerateAccessToken(user.Id, user.Username, session.SessionId);
        var refresh = _tokenService.GenerateRefreshToken();

        await _sessionCache.RemoveSessionAsync(request.RefreshToken);
        await _sessionCache.SetSessionAsync(refresh, session with { RefreshToken = refresh },
            TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        return new TokenResponse
        {
            AccessToken = access.Token,
            RefreshToken = refresh,
            ExpiresAtSeconds = new DateTimeOffset(access.ExpiresAt).ToUnixTimeSeconds(),
            TokenType = "Bearer"
        };
    }

    public override async Task<MfaChallengeResponse> InitiateMfaChallenge(MfaChallengeRequest request, ServerCallContext context)
    {
        var session = await _sessionCache.GetSessionAsync(request.SessionId);
        if (session is null)
        {
            return new MfaChallengeResponse { Success = false, Message = "Session not found" };
        }

        var user = await _users.GetUserWithMfaSettingsAsync(session.UserId, context.CancellationToken);
        if (user?.MfaSettings is null || !_mfaService.ValidateCode(user.MfaSettings.MfaSecret, request.MfaCode))
        {
            return new MfaChallengeResponse { Success = false, Message = "Invalid MFA code" };
        }

        var access = _tokenService.GenerateAccessToken(user.Id, user.Username, session.SessionId);
        var refresh = _tokenService.GenerateRefreshToken();
        await _sessionCache.SetSessionAsync(refresh, session with { RefreshToken = refresh },
            TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        return new MfaChallengeResponse
        {
            Success = true,
            Message = "MFA verified",
            AccessToken = access.Token,
            RefreshToken = refresh
        };
    }

    private async Task<LoginResponse> IssueSessionAsync(User user, bool mfaUsed, ServerCallContext context)
    {
        var sessionId = Guid.NewGuid().ToString();
        var access = _tokenService.GenerateAccessToken(user.Id, user.Username, sessionId);
        var refresh = _tokenService.GenerateRefreshToken();

        var httpContext = context.GetHttpContext();
        var session = new SessionState
        {
            UserId = user.Id,
            SessionId = sessionId,
            RefreshToken = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
        };

        await _sessionCache.SetSessionAsync(refresh, session, TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        await _eventBus.PublishAsync(new UserLoggedInEvent
        {
            CorrelationId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            UserId = user.Id,
            Username = user.Username,
            SessionId = sessionId,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            MfaUsed = mfaUsed
        }, context.CancellationToken);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = access.Token,
            RefreshToken = refresh,
            SessionId = sessionId,
            MfaRequired = false,
            ExpiresAtSeconds = new DateTimeOffset(access.ExpiresAt).ToUnixTimeSeconds()
        };
    }
}
