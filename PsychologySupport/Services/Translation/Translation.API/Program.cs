using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Translation.API.Extensions;
using Translation.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Translation");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);
services.AddExceptionHandler<CustomExceptionHandler>();


// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

// Apply CORS policy
app.UseCors("CorsPolicy");

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
        c.SwaggerEndpoint("/translation-service/swagger/v1/swagger.json", "Translation API v1");
    });
}

// Map gRPC services
app.MapGrpcService<TranslationService>();
app.MapGrpcReflectionService(); // Tùy chọn, để hỗ trợ phản xạ gRPC

// Configure health checks with UI
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Default route for non-gRPC clients
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();