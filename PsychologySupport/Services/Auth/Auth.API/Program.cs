using Auth.API.Extensions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var services = builder.Services;

services.AddCarter();
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

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