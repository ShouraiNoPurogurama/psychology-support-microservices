using BuildingBlocks.Exceptions.Handler;
using Carter;
using MassTransit;
using Profile.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddCarter();

services.AddApplicationServices(builder.Configuration);

services.AddExceptionHandler<CustomExceptionHandler>();

services.RegisterMapsterConfiguration();

//RabbitMQ
services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.PrefetchCount = 10;
    });
});


// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => {});

app.UseStaticFiles();

app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Profile API v1");
    });
}


// Apply CORS policy
app.UseCors();

app.Run();