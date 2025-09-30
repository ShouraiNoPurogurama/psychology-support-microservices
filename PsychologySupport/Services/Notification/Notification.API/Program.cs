using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Notification.API.Domains.Emails.Services;
using Notification.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Notification");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

services.ConfigureEmailFeature(builder.Configuration);

services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

app.MapCarter();

app.UseSwagger();

app.InitializeDatabaseAsync();

app.UseSwaggerUI();

//Map gRPC services
app.MapGrpcService<NotificationService>();
app.MapGrpcReflectionService(); // Tùy chọn, để hỗ trợ phản xạ gRPC


app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

// Default route for non-gRPC clients
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.UseCors("CorsPolicy");

app.Run();