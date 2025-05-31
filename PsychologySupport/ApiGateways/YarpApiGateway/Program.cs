using System.Threading.RateLimiting;
using YarpApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP
    options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("/https/aspnetapp.pfx", "12345");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("FixedWindowPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // max 100 requests
                Window = TimeSpan.FromMinutes(1), // per minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();