using BuildingBlocks.Exceptions.Handler;
using Carter;
using LifeStyles.API.Events;
using LifeStyles.API.Extensions;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddCarter();

services.AddApplicationServices(builder.Configuration);

services.AddExceptionHandler<CustomExceptionHandler>();

services.RegisterMapsterConfiguration();

// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
    });

    x.AddRequestClient<CheckPatientProfileExistenceEvent>();
});


// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LifeStyles API v1");
    });
}

app.Run();