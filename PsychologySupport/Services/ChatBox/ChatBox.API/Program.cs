using ChatBox.API.Extensions;
using ChatBox.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddApplicationServices(builder.Configuration);
services.AddSignalR();
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("CorsPolicy"); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();