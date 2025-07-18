using Auth.API.Extensions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Auth");

// Add services to the container
var services = builder.Services;

services.AddExceptionHandler<CustomExceptionHandler>();

services.AddApplicationServices(builder.Configuration, builder.Environment);
services.AddIdentityServices(builder.Configuration);

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

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
        c.SwaggerEndpoint("/auth-service/swagger/v1/swagger.json", "Auth API v1");
    });
}

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

// Apply CORS policy
app.UseCors(config =>
{
    config.AllowAnyHeader();
    config.AllowAnyMethod();
    config.AllowAnyOrigin();
});

app.MapControllers();

app.Run();