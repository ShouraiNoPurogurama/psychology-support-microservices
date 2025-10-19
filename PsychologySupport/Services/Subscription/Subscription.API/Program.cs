using BuildingBlocks.Behaviors;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Subscription.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Subscription");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

// Configure the HTTP request pipeline
var app = builder.Build();

// Exception handler, CORS, Static files
app.UseExceptionHandler(options => { });
app.UseCors("CorsPolicy");
app.UseStaticFiles();

// Routing 
//app.UseRouting();

// Authentication & Authorization 
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints / Carter
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
        c.SwaggerEndpoint("/subscription-service/swagger/v1/swagger.json", "Subscription API v1");
    });
}

// Health checks
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
