using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Profile.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Profile");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

services.AddExceptionHandler<CustomExceptionHandler>();

services.RegisterMapsterConfiguration();

// Configure the HTTP request pipeline
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
        c.SwaggerEndpoint("/profile-service/swagger/v1/swagger.json", "Profile API v1");
    });
}

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.Run();