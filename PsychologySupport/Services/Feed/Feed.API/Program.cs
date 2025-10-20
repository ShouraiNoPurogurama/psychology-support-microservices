using BuildingBlocks.Behaviors;
using Carter;
using Feed.API;
using Feed.API.Extensions;
using Feed.Application;
using Feed.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Feed");

var services = builder.Services;

services
    .AddApiServices(builder.Configuration, builder.Environment)
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

app.MapCarter();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Liveness: always healthy if process is running
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/feed-service/swagger/v1/swagger.json", "Feed API v1"); });
}

// app.UseHealthChecks("/health",
//     new HealthCheckOptions
//     {
//         ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//     }
// );

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.Run();