using ChatBox.API.Extensions;
using ChatBox.API.Hubs;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

var services = builder.Services;

services.AddApplicationServices(builder.Configuration);

IdentityModelEventSource.LogCompleteSecurityArtifact = true;
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;


var app = builder.Build();

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
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbox API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();