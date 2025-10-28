using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using UserMemory.API.Extensions;
using UserMemory.API.Shared.Services;
using UserMemory.API.Shared.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);
builder.Host.UseCustomSerilog(builder.Configuration, "UserMemory");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);
services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sync = scope.ServiceProvider.GetRequiredService<ITagSyncService>();
    await sync.SyncAsync();
}

app.UseExceptionHandler(options => { });

app.MapCarter();
    
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/user-memory-service/swagger/v1/swagger.json", "User Memory API v1");
    });
}

app.MapGrpcService<UserMemorySearchService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.Run();