using Auth.API.Extensions;
using BuildingBlocks.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var services = builder.Services;

//Cross-cutting concerns
services.AddExceptionHandler<CustomExceptionHandler>();

services.AddApplicationServices(builder.Configuration);
services.AddIdentityServices(builder.Configuration);


// Configure the HTTP request pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
}

app.Run();