using BuildingBlocks.Behaviors;
using Carter;
using Feed.API.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Feed");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

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
        c.SwaggerEndpoint("/feed-service/swagger/v1/swagger.json", "Feed API v1");
    });
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