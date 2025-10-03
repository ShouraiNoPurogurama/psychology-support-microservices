using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Translation.API.Domains.Translations.Services;
using Translation.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration.LoadConfiguration(builder.Environment);

// Serilog
builder.Host.UseCustomSerilog(builder.Configuration, "Translation");

var services = builder.Services;

// Application services
services.AddApplicationServices(builder.Configuration, builder.Environment);

// Custom exception handler
services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Exception handler middleware
app.UseExceptionHandler(options => { });

// Enable CORS
app.UseCors("CorsPolicy");

// Static files
app.UseStaticFiles();

// Carter
app.MapCarter();

// Swagger
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

// gRPC-Web middleware
app.UseGrpcWeb();

// Map gRPC services
app.MapGrpcService<TranslationService>()
    .EnableGrpcWeb()
    .RequireCors("CorsPolicy");

app.MapGrpcReflectionService()
    .EnableGrpcWeb();

// Health checks
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Default GET route
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
