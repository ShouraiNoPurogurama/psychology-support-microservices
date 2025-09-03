using Billing.API.Extensions;
using BuildingBlocks.Behaviors;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Subscription");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.MapCarter();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    //app.InitializeDatabaseAsync();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/billing-service/swagger/v1/swagger.json", "Billing API v1");
    });
}

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.Run();