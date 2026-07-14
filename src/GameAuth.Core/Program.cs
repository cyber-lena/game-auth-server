using System.Text;
using GameAuth.Core.Configuration;
using GameAuth.Core.Security;
using GameAuth.Core.Services;
using GameAuth.Infrastructure;
using GameAuth.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string ServiceName = "GameAuth.Core";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDefaults(ServiceName);

builder.Services.AddServiceDefaults(builder.Configuration, ServiceName);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IMfaService, TotpMfaService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddGrpc();

var app = builder.Build();

app.UseServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AuthGrpcService>();

app.Run();

