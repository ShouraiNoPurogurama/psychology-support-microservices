using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using ChatBox.API.Extensions;
using ChatBox.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);
builder.Host.UseStandardSerilog(builder.Configuration, "ChatBox Service");

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
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbox API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();