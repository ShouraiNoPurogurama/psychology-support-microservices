using AIModeration.API.Extensions;
using AIModeration.API.Features.Aliases;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);
builder.Host.UseCustomSerilog(builder.Configuration, "Moderation");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);
services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler(options => { });
app.UseCors("CorsPolicy");

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
    app.UseSwaggerUI(); 
}
else
{
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/moderation-service/swagger/v1/swagger.json", "Moderation API v1"); });
}

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

app.MapGrpcService<ModerationGrpcService>();
app.MapGrpcReflectionService();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();


app.Run();