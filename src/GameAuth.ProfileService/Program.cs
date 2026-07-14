using GameAuth.Infrastructure;
using GameAuth.ProfileService.Consumers;
using GameAuth.ProfileService.Services;
using GameAuth.ProfileService.Storage;
using GameAuth.ServiceDefaults;

const string ServiceName = "GameAuth.ProfileService";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDefaults(ServiceName);

builder.Services.AddServiceDefaults(builder.Configuration, ServiceName);
builder.Services.AddInfrastructure(builder.Configuration, mt =>
{
    mt.AddConsumer<UserRegisteredConsumer>();
});

builder.Services.AddScoped<IProfileStore, RedisProfileStore>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseServiceDefaults();

app.MapGrpcService<ProfileGrpcService>();

app.Run();

