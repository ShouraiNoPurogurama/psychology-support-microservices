using System.Security.Cryptography.X509Certificates;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using YarpApiGateway.Extensions;
using YarpApiGateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(
                "/certs/fullchain.pem",
                "/certs/privkey.pem"
            );
        });

        serverOptions.ListenAnyIP(80);
        serverOptions.ListenAnyIP(443, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
}

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);


var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// app.UseMiddleware<LoggingMiddleware>();
app.MapReverseProxy();

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

app.Run();