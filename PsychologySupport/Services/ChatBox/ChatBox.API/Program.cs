using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using ChatBox.API.Domains.Chatboxes.Hubs;
using ChatBox.API.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);
builder.Host.UseCustomSerilog(builder.Configuration, "ChatBox");

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/chatbox-service/swagger/v1/swagger.json", "Chatbox API v1");
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
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();