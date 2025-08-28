using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Notification.API.Extensions;
using Notification.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.WebHost.ConfigureKestrel(options =>
{
    //Chỉ set protocols, còn port sẽ lấy từ cấu hình (launchSettings, env vars, appsettings.json)
    options.ConfigureEndpointDefaults(lo =>
    {
        lo.Protocols = HttpProtocols.Http1AndHttp2;
    });
});


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
if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/notification-service/swagger/v1/swagger.json", "Scheduling API v1");
    });
}

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