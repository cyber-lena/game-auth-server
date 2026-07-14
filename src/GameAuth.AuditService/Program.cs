using GameAuth.AuditService.Consumers;
using GameAuth.AuditService.Services;
using GameAuth.AuditService.Storage;
using GameAuth.Infrastructure;
using GameAuth.ServiceDefaults;

const string ServiceName = "GameAuth.AuditService";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDefaults(ServiceName);

builder.Services.AddServiceDefaults(builder.Configuration, ServiceName);
builder.Services.AddInfrastructure(builder.Configuration, mt =>
{
    mt.AddConsumer<UserLoggedInAuditConsumer>();
    mt.AddConsumer<UserRegisteredAuditConsumer>();
    mt.AddConsumer<SecurityEventAuditConsumer>();
});

builder.Services.AddScoped<IAuditLogStore, AuditLogStore>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseServiceDefaults();

app.MapGrpcService<AuditGrpcService>();

app.Run();

